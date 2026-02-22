using Prompter.Core.Enums;

namespace Prompter.Core.Entities;

public class Prompt
{
    public int Id { get; private set; }
    public string Text { get; private set; }
    public PromptStatus Status { get; private set; }
    public string? Response { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private Prompt() { Text = string.Empty; }

    public Prompt(string text)
    {
        Text = text;
        Status = PromptStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessing()
    {
        Status = PromptStatus.Processing;
    }

    public void Complete(string response)
    {
        Response = response;
        Status = PromptStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = PromptStatus.Failed;
        CompletedAt = DateTime.UtcNow;
    }
}
