using Microsoft.EntityFrameworkCore;
using Prompter.Core.Entities;

namespace Prompter.Data;

public class PrompterDbContext(DbContextOptions<PrompterDbContext> options) : DbContext(options)
{
    public DbSet<Prompt> Prompts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PrompterDbContext).Assembly);
    }
}
