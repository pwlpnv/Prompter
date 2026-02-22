using Prompter.Core.Entities;
using Prompter.Core.Models;

namespace Prompter.Core.Repositories;

public interface IPromptRepository
{
    Task AddRangeAsync(IEnumerable<Prompt> prompts, CancellationToken cancellationToken = default);
    Task<Prompt?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<Prompt>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}
