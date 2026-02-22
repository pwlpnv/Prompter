using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Prompter.Core.Entities;
using Prompter.Core.Enums;
using Prompter.Core.Services;
using Prompter.Services;

namespace Prompter.Tests;

public class PromptProcessorTests
{
    private readonly ILlmClient _llmClient = Substitute.For<ILlmClient>();
    private readonly ILogger<PromptProcessor> _logger = Substitute.For<ILogger<PromptProcessor>>();
    private readonly PromptProcessor _sut;

    public PromptProcessorTests()
    {
        _sut = new PromptProcessor(_llmClient, _logger);
    }

    [Fact]
    public async Task ProcessAsync_SuccessfulResponse_CompletesPrompt()
    {
        var prompt = new Prompt("test");
        prompt.Process();
        _llmClient.GenerateAsync("test", Arg.Any<CancellationToken>())
            .Returns("AI response");

        await _sut.ProcessAsync(prompt);

        prompt.Status.Should().Be(PromptStatus.Completed);
        prompt.Response.Should().Be("AI response");
    }

    [Fact]
    public async Task ProcessAsync_LlmThrows_FailsPrompt()
    {
        var prompt = new Prompt("test");
        prompt.Process();
        _llmClient.GenerateAsync("test", Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("LLM error"));

        await _sut.ProcessAsync(prompt);

        prompt.Status.Should().Be(PromptStatus.Failed);
        prompt.Response.Should().BeNull();
    }

    [Fact]
    public async Task ProcessAsync_OperationCancelled_PropagatesException()
    {
        var prompt = new Prompt("test");
        prompt.Process();
        _llmClient.GenerateAsync("test", Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        var act = () => _sut.ProcessAsync(prompt);

        await act.Should().ThrowAsync<OperationCanceledException>();
        prompt.Status.Should().Be(PromptStatus.Processing);
    }
}
