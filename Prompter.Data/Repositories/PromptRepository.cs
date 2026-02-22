using Microsoft.EntityFrameworkCore;
using Prompter.Core.Entities;
using Prompter.Core.Enums;
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

    public async Task<IEnumerable<Prompt>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Prompts
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Prompt?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Prompts.FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<Prompt>> GetByStatusAsync(PromptStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Prompts
            .Where(p => p.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Prompt prompt, CancellationToken cancellationToken = default)
    {
        _context.Prompts.Update(prompt);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Prompt> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _context.Prompts.OrderByDescending(p => p.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
