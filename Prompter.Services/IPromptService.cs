using Prompter.Core.Entities;

namespace Prompter.Services;

public interface IPromptService
{
    Task<IEnumerable<Prompt>> CreatePromptsAsync(string[] promptTexts, CancellationToken cancellationToken = default);
    Task<IEnumerable<Prompt>> GetAllPromptsAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Prompt> Items, int TotalCount)> GetPromptsPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}
