using FluentAssertions;
using Prompter.Core.Entities;
using Prompter.Core.Enums;

namespace Prompter.Tests;

public class PromptTests
{
    [Fact]
    public void Constructor_SetsTextAndDefaults()
    {
        var prompt = new Prompt("Hello AI");

        prompt.Text.Should().Be("Hello AI");
        prompt.Status.Should().Be(PromptStatus.Pending);
        prompt.Response.Should().BeNull();
        prompt.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        prompt.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Process_WhenPending_SetsStatusToProcessing()
    {
        var prompt = new Prompt("test");

        prompt.Process();

        prompt.Status.Should().Be(PromptStatus.Processing);
    }

    [Fact]
    public void Process_WhenAlreadyProcessing_ResetsTimestamp()
    {
        var prompt = new Prompt("test");
        prompt.Process();
        var firstTimestamp = prompt.StartedProcessingAt;

        prompt.Process();

        prompt.Status.Should().Be(PromptStatus.Processing);
        prompt.StartedProcessingAt.Should().BeOnOrAfter(firstTimestamp!.Value);
    }

    [Theory]
    [InlineData(PromptStatus.Completed)]
    [InlineData(PromptStatus.Failed)]
    public void Process_WhenCompletedOrFailed_Throws(PromptStatus initialStatus)
    {
        var prompt = CreatePromptWithStatus(initialStatus);

        var act = () => prompt.Process();

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Complete_WhenProcessing_SetsResponseAndStatus()
    {
        var prompt = new Prompt("test");
        prompt.Process();

        prompt.Complete("AI response");

        prompt.Status.Should().Be(PromptStatus.Completed);
        prompt.Response.Should().Be("AI response");
        prompt.CompletedAt.Should().NotBeNull();
        prompt.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(PromptStatus.Pending)]
    [InlineData(PromptStatus.Completed)]
    [InlineData(PromptStatus.Failed)]
    public void Complete_WhenNotProcessing_Throws(PromptStatus initialStatus)
    {
        var prompt = CreatePromptWithStatus(initialStatus);

        var act = () => prompt.Complete("response");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Complete_WithNullOrEmptyResponse_Throws(string? response)
    {
        var prompt = new Prompt("test");
        prompt.Process();

        var act = () => prompt.Complete(response!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Fail_WhenProcessing_SetsStatusToFailed()
    {
        var prompt = new Prompt("test");
        prompt.Process();

        prompt.Fail();

        prompt.Status.Should().Be(PromptStatus.Failed);
        prompt.CompletedAt.Should().NotBeNull();
        prompt.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(PromptStatus.Pending)]
    [InlineData(PromptStatus.Completed)]
    [InlineData(PromptStatus.Failed)]
    public void Fail_WhenNotProcessing_Throws(PromptStatus initialStatus)
    {
        var prompt = CreatePromptWithStatus(initialStatus);

        var act = () => prompt.Fail();

        act.Should().Throw<ArgumentException>();
    }

    private static Prompt CreatePromptWithStatus(PromptStatus status)
    {
        var prompt = new Prompt("test");

        if (status is PromptStatus.Processing or PromptStatus.Completed or PromptStatus.Failed)
            prompt.Process();

        if (status == PromptStatus.Completed)
            prompt.Complete("done");

        if (status == PromptStatus.Failed)
            prompt.Fail();

        return prompt;
    }
}
