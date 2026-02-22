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
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) return BadRequest("page must be at least 1.");
        if (pageSize < 1) return BadRequest("pageSize must be at least 1.");

        var skip = (page - 1) * pageSize;
        var result = await _promptService.GetPromptsPagedAsync(skip, pageSize, cancellationToken);
        return Ok(new PagedResponse<PromptDetails>(result.Items.Select(ToDto), result.TotalCount));
    }

    // Simple inline mapping; for more complex scenarios we would use AutoMapper or a dedicated mapping class.
    private static PromptDetails ToDto(Prompt p) =>
        new(p.Id, p.Text, p.Status, p.Response, p.CreatedAt, p.CompletedAt);
}
