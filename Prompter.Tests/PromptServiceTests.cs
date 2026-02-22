using FluentAssertions;
using NSubstitute;
using Prompter.Core.Entities;
using Prompter.Core.Models;
using Prompter.Core.Repositories;
using Prompter.Core.UnitOfWork;
using Prompter.Services;

namespace Prompter.Tests;

public class PromptServiceTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPromptRepository _promptRepository = Substitute.For<IPromptRepository>();
    private readonly PromptService _sut;

    public PromptServiceTests()
    {
        _unitOfWork.Prompts.Returns(_promptRepository);
        _sut = new PromptService(_unitOfWork);
    }

    [Fact]
    public async Task CreatePromptsAsync_ValidTexts_AddsAndSaves()
    {
        var texts = new[] { "Hello", "World" };

        await _sut.CreatePromptsAsync(texts);

        await _promptRepository.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<Prompt>>(p => p.Count() == 2),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreatePromptsAsync_ValidTexts_ReturnsCreatedPrompts()
    {
        var texts = new[] { "First", "Second" };

        var result = await _sut.CreatePromptsAsync(texts);

        result.Should().HaveCount(2);
        result.Select(p => p.Text).Should().BeEquivalentTo(texts);
    }

    [Fact]
    public async Task CreatePromptsAsync_NullArray_ThrowsWithoutSaving()
    {
        var act = () => _sut.CreatePromptsAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreatePromptsAsync_EmptyArray_ThrowsWithoutSaving()
    {
        var act = () => _sut.CreatePromptsAsync([]);

        await act.Should().ThrowAsync<ArgumentException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  \t  ")]
    public async Task CreatePromptsAsync_WhitespaceText_ThrowsWithoutSaving(string whitespace)
    {
        var act = () => _sut.CreatePromptsAsync([whitespace]);

        await act.Should().ThrowAsync<ArgumentException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPromptsPagedAsync_DelegatesToRepository()
    {
        var expected = new PagedResult<Prompt>(
            [new Prompt("test")], TotalCount: 1);

        _promptRepository.GetPagedAsync(0, 10, Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.GetPromptsPagedAsync(0, 10);

        result.Should().BeSameAs(expected);
        await _promptRepository.Received(1).GetPagedAsync(0, 10, Arg.Any<CancellationToken>());
    }
}
