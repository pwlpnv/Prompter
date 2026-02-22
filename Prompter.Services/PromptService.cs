using Prompter.Core.Entities;
using Prompter.Core.Repositories;

namespace Prompter.Services;

public class PromptService : IPromptService
{
    private readonly IPromptRepository _repository;

    public PromptService(IPromptRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Prompt>> CreatePromptsAsync(string[] promptTexts, CancellationToken cancellationToken = default)
    {
        var prompts = promptTexts.Select(text => new Prompt(text)).ToList();
        await _repository.AddRangeAsync(prompts, cancellationToken);
        return prompts;
    }

    public async Task<IEnumerable<Prompt>> GetAllPromptsAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Prompt> Items, int TotalCount)> GetPromptsPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _repository.GetPagedAsync(skip, take, cancellationToken);
    }
}
