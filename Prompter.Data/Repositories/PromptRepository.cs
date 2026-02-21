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

    public async Task AddRangeAsync(IEnumerable<Prompt> prompts)
    {
        await _context.Prompts.AddRangeAsync(prompts);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Prompt>> GetAllAsync()
    {
        return await _context.Prompts
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Prompt?> GetByIdAsync(int id)
    {
        return await _context.Prompts.FindAsync(id);
    }

    public async Task<IEnumerable<Prompt>> GetByStatusAsync(PromptStatus status)
    {
        return await _context.Prompts
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    public async Task UpdateAsync(Prompt prompt)
    {
        _context.Prompts.Update(prompt);
        await _context.SaveChangesAsync();
    }
}
