using Microsoft.EntityFrameworkCore;
using Prompter.Core.Entities;
using Prompter.Core.Enums;
using Prompter.Core.Models;
using Prompter.Core.Repositories;

namespace Prompter.Data.Repositories;

public class PromptRepository(PrompterDbContext context) : IPromptRepository
{
    public async Task AddRangeAsync(IEnumerable<Prompt> prompts, CancellationToken cancellationToken = default)
    {
        await context.Prompts.AddRangeAsync(prompts, cancellationToken);
    }

    public async Task<IReadOnlyList<Prompt>> ClaimPendingAsync(int take, TimeSpan staleTimeout, CancellationToken cancellationToken = default)
    {
        var staleThreshold = DateTime.UtcNow - staleTimeout;

        return await context.Prompts
            .FromSqlRaw(
                """
                SELECT * FROM "Prompts"
                WHERE "Status" = 'Pending'
                   OR ("Status" = 'Processing' AND "StartedProcessingAt" < {1})
                ORDER BY "CreatedAt"
                LIMIT {0}
                FOR UPDATE SKIP LOCKED
                """, take, staleThreshold)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Prompt>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = context.Prompts.OrderByDescending(p => p.CreatedAt);
        
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
        
        return new PagedResult<Prompt>(items, totalCount);
    }
}
