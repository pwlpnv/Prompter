using Microsoft.EntityFrameworkCore;
using Prompter.Core.Entities;
using Prompter.Core.Models;
using Prompter.Core.Repositories;

namespace Prompter.Data.Repositories;

public class PromptRepository(PrompterDbContext context) : IPromptRepository
{
    public async Task AddRangeAsync(IEnumerable<Prompt> prompts, CancellationToken cancellationToken = default)
    {
        await context.Prompts.AddRangeAsync(prompts, cancellationToken);
    }

    public async Task<Prompt?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Prompts.FindAsync([id], cancellationToken);
    }

    public async Task<PagedResult<Prompt>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = context.Prompts.OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);

        return new PagedResult<Prompt>(items, totalCount);
    }
}
