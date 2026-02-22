using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prompter.Core.Entities;

namespace Prompter.Data.Configurations;

public class PromptConfiguration : IEntityTypeConfiguration<Prompt>
{
    public void Configure(EntityTypeBuilder<Prompt> builder)
    {
        builder.ToTable("Prompts");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Text)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.Response)
            .HasMaxLength(10000);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.StartedProcessingAt);

        builder.HasIndex(p => p.Status);
    }
}
