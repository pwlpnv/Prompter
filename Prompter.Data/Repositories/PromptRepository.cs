using Microsoft.EntityFrameworkCore;
using Prompter.Core.Entities;
using Prompter.Core.Enums;
using Prompter.Core.Models;
using Prompter.Core.Repositories;

namespace Prompter.Data.Repositories;

public class PromptRepository : IPromptRepository
{
    private readonly PrompterDbContext _context;

    public PromptRepository(PrompterDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<Prompt> prompts, CancellationToken cancellationToken = default)
    {
        await _context.Prompts.AddRangeAsync(prompts, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Prompt?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Prompts.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<Prompt>> GetByStatusAsync(PromptStatus status, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Prompts
            .Where(p => p.Status == status)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prompt>> ClaimPendingAsync(int take, CancellationToken cancellationToken = default)
    {
        return await _context.Prompts
            .FromSqlRaw(
                """
                SELECT * FROM "Prompts"
                WHERE "Status" = 'Pending'
                ORDER BY "CreatedAt"
                LIMIT {0}
                FOR UPDATE SKIP LOCKED
                """, take)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.RollbackTransactionAsync(cancellationToken);
    }

    public async Task UpdateAsync(Prompt prompt, CancellationToken cancellationToken = default)
    {
        _context.Prompts.Update(prompt);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<Prompt>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _context.Prompts.OrderByDescending(p => p.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
        return new PagedResult<Prompt>(items, totalCount);
    }
}
