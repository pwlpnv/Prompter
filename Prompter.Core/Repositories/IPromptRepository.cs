using Prompter.Core.Entities;
using Prompter.Core.Enums;

namespace Prompter.Core.Repositories;

public interface IPromptRepository
{
    Task AddRangeAsync(IEnumerable<Prompt> prompts, CancellationToken cancellationToken = default);
    Task<IEnumerable<Prompt>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Prompt?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Prompt>> GetByStatusAsync(PromptStatus status, CancellationToken cancellationToken = default);
    Task UpdateAsync(Prompt prompt, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Prompt> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}
