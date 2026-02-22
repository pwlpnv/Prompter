using Prompter.Core.Repositories;
using Prompter.Core.UnitOfWork;
using Prompter.Data.Repositories;

namespace Prompter.Data.UnitOfWork;

public class UnitOfWork(PrompterDbContext context) : IUnitOfWork
{
    public IPromptRepository Prompts { get; } = new PromptRepository(context);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        await context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
        await context.Database.CommitTransactionAsync(cancellationToken);

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default) =>
        await context.Database.RollbackTransactionAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (context.Database.CurrentTransaction is not null)
            await context.Database.CurrentTransaction.DisposeAsync();
    }
}
