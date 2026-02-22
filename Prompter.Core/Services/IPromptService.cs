using Prompter.Core.Entities;
using Prompter.Core.Models;

namespace Prompter.Core.Services;

public interface IPromptService
{
    Task<IEnumerable<Prompt>> CreatePromptsAsync(string[] promptTexts, CancellationToken cancellationToken = default);
    Task<PagedResult<Prompt>> GetPromptsPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}
