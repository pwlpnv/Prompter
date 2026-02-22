using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Prompter.Core.Entities;
using Prompter.Core.Enums;
using Prompter.Core.Messages;
using Prompter.Core.Repositories;
using Prompter.Core.Services;
using Prompter.Core.UnitOfWork;
using Prompter.Worker.Consumers;

namespace Prompter.Tests;

public class ProcessPromptConsumerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPromptRepository _promptRepository = Substitute.For<IPromptRepository>();
    private readonly ILlmClient _llmClient = Substitute.For<ILlmClient>();
    private readonly ProcessPromptConsumer _sut;

    public ProcessPromptConsumerTests()
    {
        _unitOfWork.Prompts.Returns(_promptRepository);
        _sut = new ProcessPromptConsumer(
            _unitOfWork,
            _llmClient,
            Substitute.For<ILogger<ProcessPromptConsumer>>());
    }

    private static ConsumeContext<ProcessPrompt> CreateContext(int promptId)
    {
        var context = Substitute.For<ConsumeContext<ProcessPrompt>>();
        context.Message.Returns(new ProcessPrompt(promptId));
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }

    [Fact]
    public async Task Consume_PromptNotFound_Throws()
    {
        _promptRepository.GetByIdAsync(42, Arg.Any<CancellationToken>())
            .Returns((Prompt?)null);

        var act = () => _sut.Consume(CreateContext(42));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*42*");
        await _llmClient.DidNotReceive()
            .GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WithPrompt_CallsLlmAndCompletes()
    {
        var prompt = new Prompt("test");
        _promptRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(prompt);
        _llmClient.GenerateAsync("test", Arg.Any<CancellationToken>())
            .Returns("response");

        await _sut.Consume(CreateContext(1));

        prompt.Status.Should().Be(PromptStatus.Completed);
        prompt.Response.Should().Be("response");
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_LlmThrows_PropagatesForRetry()
    {
        var prompt = new Prompt("test");
        _promptRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(prompt);
        _llmClient.GenerateAsync("test", Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("LLM error"));

        var act = () => _sut.Consume(CreateContext(1));

        await act.Should().ThrowAsync<InvalidOperationException>();
        prompt.Status.Should().Be(PromptStatus.Processing);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class ProcessPromptFaultConsumerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPromptRepository _promptRepository = Substitute.For<IPromptRepository>();
    private readonly ProcessPromptFaultConsumer _sut;

    public ProcessPromptFaultConsumerTests()
    {
        _unitOfWork.Prompts.Returns(_promptRepository);
        _sut = new ProcessPromptFaultConsumer(
            _unitOfWork,
            Substitute.For<ILogger<ProcessPromptFaultConsumer>>());
    }

    private static ConsumeContext<Fault<ProcessPrompt>> CreateFaultContext(int promptId)
    {
        var context = Substitute.For<ConsumeContext<Fault<ProcessPrompt>>>();
        var fault = Substitute.For<Fault<ProcessPrompt>>();
        fault.Message.Returns(new ProcessPrompt(promptId));
        context.Message.Returns(fault);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }

    [Fact]
    public async Task Consume_ProcessingPrompt_MarksAsFailed()
    {
        var prompt = new Prompt("test");
        prompt.Process();
        _promptRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(prompt);

        await _sut.Consume(CreateFaultContext(1));

        prompt.Status.Should().Be(PromptStatus.Failed);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_PromptNotFound_DoesNothing()
    {
        _promptRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns((Prompt?)null);

        await _sut.Consume(CreateFaultContext(1));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_AlreadyCompleted_DoesNotOverwrite()
    {
        var prompt = new Prompt("test");
        prompt.Process();
        prompt.Complete("done");
        _promptRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(prompt);

        await _sut.Consume(CreateFaultContext(1));

        prompt.Status.Should().Be(PromptStatus.Completed);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
