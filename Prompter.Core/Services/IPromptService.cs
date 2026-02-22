using Prompter.Core.Entities;

namespace Prompter.Core.Services;

public interface IPromptService
{
    Task<IEnumerable<Prompt>> CreatePromptsAsync(string[] promptTexts);
    Task<IEnumerable<Prompt>> GetAllPromptsAsync();
    Task<(IEnumerable<Prompt> Items, int TotalCount)> GetPromptsPagedAsync(int skip, int take);
}
