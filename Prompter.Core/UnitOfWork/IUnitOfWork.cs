using Prompter.Core.Repositories;

namespace Prompter.Core.UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable
{
    IPromptRepository Prompts { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
