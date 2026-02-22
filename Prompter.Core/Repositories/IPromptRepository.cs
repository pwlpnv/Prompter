using Prompter.Core.Entities;
using Prompter.Core.Enums;
using Prompter.Core.Models;

namespace Prompter.Core.Repositories;

public interface IPromptRepository
{
    Task AddRangeAsync(IEnumerable<Prompt> prompts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects pending prompts with row-level locking (FOR UPDATE SKIP LOCKED) so multiple
    /// worker instances can safely poll without processing the same prompt twice.
    /// Must be called within an active transaction.
    /// For higher throughput, consider replacing DB polling with RabbitMQ via MassTransit.
    /// </summary>
    Task<IReadOnlyList<Prompt>> ClaimPendingAsync(int take, TimeSpan staleTimeout, CancellationToken cancellationToken = default);

    Task<PagedResult<Prompt>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}
