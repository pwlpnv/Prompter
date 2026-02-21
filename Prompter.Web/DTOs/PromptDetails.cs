using Prompter.Core.Enums;

namespace Prompter.Web.DTOs;

public record PromptDetails(
    int Id,
    string Prompt,
    PromptStatus Status,
    string? Response,
    DateTime CreatedAt,
    DateTime? CompletedAt);
