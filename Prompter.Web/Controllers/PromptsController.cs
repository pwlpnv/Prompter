using Microsoft.AspNetCore.Mvc;
using Prompter.Core.Entities;
using Prompter.Services;
using Prompter.Web.DTOs;

namespace Prompter.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromptsController : ControllerBase
{
    private readonly IPromptService _promptService;

    public PromptsController(IPromptService promptService)
    {
        _promptService = promptService;
    }

    [HttpPost]
    public async Task<ActionResult<IEnumerable<PromptDetails>>> CreatePrompts(
        [FromBody] CreatePromptsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Prompts is null || request.Prompts.Length == 0)
            return BadRequest("At least one prompt is required.");

        if (request.Prompts.Length > 50)
            return BadRequest("Maximum 50 prompts per request.");

        var validPrompts = request.Prompts
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        if (validPrompts.Length == 0)
            return BadRequest("All prompts are empty or whitespace.");

        var tooLong = validPrompts.FirstOrDefault(p => p.Length > 4000);
        if (tooLong is not null)
            return BadRequest("Each prompt must be 4000 characters or fewer.");

        var prompts = await _promptService.CreatePromptsAsync(validPrompts, cancellationToken);
        return StatusCode(201, prompts.Select(ToDto));
    }

    [HttpGet]
    public async Task<IActionResult> GetPrompts(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        if (page.HasValue && pageSize.HasValue)
        {
            var skip = (page.Value - 1) * pageSize.Value;
            var (items, totalCount) = await _promptService.GetPromptsPagedAsync(skip, pageSize.Value, cancellationToken);
            return Ok(new PagedResponse<PromptDetails>(items.Select(ToDto), totalCount));
        }

        var prompts = await _promptService.GetAllPromptsAsync(cancellationToken);
        return Ok(prompts.Select(ToDto));
    }

    private static PromptDetails ToDto(Prompt p) =>
        new(p.Id, p.Text, p.Status, p.Response, p.CreatedAt, p.CompletedAt);
}
