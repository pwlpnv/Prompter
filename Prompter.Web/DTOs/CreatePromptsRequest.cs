using System.ComponentModel.DataAnnotations;

namespace Prompter.Web.DTOs;

public record CreatePromptsRequest(
    [Required]
    [MinLength(1, ErrorMessage = "At least one prompt is required.")]
    [MaxLength(50, ErrorMessage = "Maximum 50 prompts per request.")]
    string[] Prompts);
