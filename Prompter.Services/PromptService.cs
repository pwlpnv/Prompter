using Ardalis.GuardClauses;
using Prompter.Core.Entities;
using Prompter.Core.Models;
using Prompter.Core.Repositories;

namespace Prompter.Services;

public class PromptService : IPromptService
{
    private readonly IPromptRepository _repository;

    public PromptService(IPromptRepository repository)
    {
        _repository = Guard.Against.Null(repository);
    }

    public async Task<IEnumerable<Prompt>> CreatePromptsAsync(string[] promptTexts, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(promptTexts);
        Guard.Against.InvalidInput(promptTexts, nameof(promptTexts), arr => arr.Length > 0, "At least one prompt is required.");
        Guard.Against.InvalidInput(promptTexts, nameof(promptTexts), arr => arr.All(t => !string.IsNullOrWhiteSpace(t)), "Prompts cannot be empty or whitespace.");

        var prompts = promptTexts.Select(text => new Prompt(text)).ToList();
        await _repository.AddRangeAsync(prompts, cancellationToken);
        return prompts;
    }

    public async Task<PagedResult<Prompt>> GetPromptsPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _repository.GetPagedAsync(skip, take, cancellationToken);
    }
}
