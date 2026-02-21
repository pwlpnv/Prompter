using Prompter.Core.Enums;

namespace Prompter.Core.Entities;

public class Prompt
{
    public int Id { get; set; }
    public string Text { get; set; }
    public PromptStatus Status { get; set; }
    public string? Response { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    private Prompt() { Text = string.Empty; }

    public Prompt(string text)
    {
        Text = text;
        Status = PromptStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }
}
