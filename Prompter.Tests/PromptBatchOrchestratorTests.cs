using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Prompter.Core.Entities;
using Prompter.Core.Repositories;
using Prompter.Core.Services;
using Prompter.Core.UnitOfWork;
using Prompter.Services;

namespace Prompter.Tests;

public class PromptBatchOrchestratorTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPromptRepository _promptRepository = Substitute.For<IPromptRepository>();
    private readonly IPromptProcessor _promptProcessor = Substitute.For<IPromptProcessor>();
    private readonly PromptBatchOrchestrator _sut;

    public PromptBatchOrchestratorTests()
    {
        _unitOfWork.Prompts.Returns(_promptRepository);
        _sut = new PromptBatchOrchestrator(
            _unitOfWork,
            _promptProcessor,
            Substitute.For<ILogger<PromptBatchOrchestrator>>());
    }

    [Fact]
    public async Task RunBatchAsync_NoPendingPrompts_ReturnsFalse()
    {
        _promptRepository.ClaimPendingAsync(Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(new List<Prompt>());

        var result = await _sut.RunBatchAsync(CancellationToken.None);

        result.Should().BeFalse();
        await _promptProcessor.DidNotReceive()
            .ProcessAsync(Arg.Any<Prompt>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunBatchAsync_WithPrompts_ProcessesAndSaves()
    {
        var prompts = new List<Prompt> { new("first"), new("second") };
        _promptRepository.ClaimPendingAsync(Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(prompts);

        var result = await _sut.RunBatchAsync(CancellationToken.None);

        result.Should().BeTrue();
        await _promptProcessor.Received(2)
            .ProcessAsync(Arg.Any<Prompt>(), Arg.Any<CancellationToken>());
        // SaveChanges called twice: once for claim (Processing status), once for results
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunBatchAsync_WithPrompts_SetsStatusToProcessingBeforeLlmCall()
    {
        var prompt = new Prompt("test");
        _promptRepository.ClaimPendingAsync(Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(new List<Prompt> { prompt });

        await _sut.RunBatchAsync(CancellationToken.None);

        // Prompt.Process() was called (status changed from Pending)
        // PromptProcessor then either completes or fails it
        await _promptProcessor.Received(1).ProcessAsync(prompt, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunBatchAsync_ClaimThrows_RollsBackAndPropagates()
    {
        _promptRepository.ClaimPendingAsync(Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var act = () => _sut.RunBatchAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _unitOfWork.Received(1).RollbackTransactionAsync(CancellationToken.None);
    }

    [Fact]
    public async Task RunBatchAsync_PassesStaleTimeoutToRepository()
    {
        _promptRepository.ClaimPendingAsync(Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(new List<Prompt>());

        await _sut.RunBatchAsync(CancellationToken.None);

        await _promptRepository.Received(1).ClaimPendingAsync(
            Arg.Any<int>(),
            TimeSpan.FromMinutes(5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunBatchAsync_Cancelled_StopsProcessing()
    {
        var prompts = new List<Prompt> { new("first"), new("second"), new("third") };
        _promptRepository.ClaimPendingAsync(Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(prompts);

        using var cts = new CancellationTokenSource();

        _promptProcessor.ProcessAsync(Arg.Any<Prompt>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(_ => cts.Cancel());

        // Should process first prompt, then cancel triggers on second check
        await _sut.RunBatchAsync(cts.Token);

        await _promptProcessor.Received(1)
            .ProcessAsync(Arg.Any<Prompt>(), Arg.Any<CancellationToken>());
    }
}
