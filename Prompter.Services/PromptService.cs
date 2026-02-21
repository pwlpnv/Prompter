using Prompter.Core.Entities;
using Prompter.Core.Repositories;
using Prompter.Core.Services;

namespace Prompter.Services;

public class PromptService : IPromptService
{
    private readonly IPromptRepository _repository;

    public PromptService(IPromptRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Prompt>> CreatePromptsAsync(string[] promptTexts)
    {
        var prompts = promptTexts.Select(text => new Prompt(text)).ToList();
        await _repository.AddRangeAsync(prompts);
        return prompts;
    }

    public async Task<IEnumerable<Prompt>> GetAllPromptsAsync()
    {
        return await _repository.GetAllAsync();
    }
}
