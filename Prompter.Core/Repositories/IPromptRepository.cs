using Prompter.Core.Entities;
using Prompter.Core.Enums;
using Prompter.Core.Models;

namespace Prompter.Core.Repositories;

public interface IPromptRepository
{
    Task AddRangeAsync(IEnumerable<Prompt> prompts, CancellationToken cancellationToken = default);
    Task<Prompt?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prompt>> GetByStatusAsync(PromptStatus status, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically claims pending prompts by setting their status to Processing.
    /// Uses row-level locking (FOR UPDATE SKIP LOCKED) so multiple worker instances
    /// can safely poll without processing the same prompt twice.
    /// For higher throughput, consider replacing DB polling with a RabbitMQ via MassTransit.
    /// </summary>
    Task<IReadOnlyList<Prompt>> ClaimPendingAsync(int take, CancellationToken cancellationToken = default);

    Task UpdateAsync(Prompt prompt, CancellationToken cancellationToken = default);
    Task<PagedResult<Prompt>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}
