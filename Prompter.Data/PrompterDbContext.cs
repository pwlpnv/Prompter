using Microsoft.EntityFrameworkCore;
using Prompter.Core.Entities;

namespace Prompter.Data;

public class PrompterDbContext : DbContext
{
    public DbSet<Prompt> Prompts { get; set; }

    public PrompterDbContext(DbContextOptions<PrompterDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PrompterDbContext).Assembly);
    }
}
