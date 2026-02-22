using Ardalis.GuardClauses;
using Prompter.Core.Enums;

namespace Prompter.Core.Entities;

public class Prompt(string text)
{
    public int Id { get; }
    public string Text { get; private set; } = text;
    public PromptStatus Status { get; private set; } = PromptStatus.Pending;
    public string? Response { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; private set; }

    public void Process()
    {
        Guard.Against.Expression(status => status != PromptStatus.Pending, Status, "Can only process a prompt that is in Pending status.");
        Status = PromptStatus.Processing;
    }

    public void Complete(string response)
    {
        Guard.Against.Expression(status => status != PromptStatus.Processing, Status, "Can only complete a prompt that is in Processing status.");
        Guard.Against.NullOrEmpty(response);
        
        Response = response;
        Status = PromptStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Guard.Against.Expression(status => status != PromptStatus.Processing, Status, "Can only fail a prompt that is in Processing status.");

        Status = PromptStatus.Failed;
        CompletedAt = DateTime.UtcNow;
    }
}
