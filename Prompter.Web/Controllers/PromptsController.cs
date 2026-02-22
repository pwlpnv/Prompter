using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Prompter.Core.Entities;
using Prompter.Core.Services;
using Prompter.Web.DTOs;

namespace Prompter.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromptsController(IPromptService promptService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<IEnumerable<PromptDetails>>> CreatePrompts(
        [FromBody] CreatePromptsRequest request,
        CancellationToken cancellationToken)
    {
        var prompts = await promptService.CreatePromptsAsync(request.Prompts, cancellationToken);
        return StatusCode(201, prompts.Select(ToDto));
    }

    [HttpGet]
    public async Task<IActionResult> GetPrompts(
        [FromQuery][Range(1, int.MaxValue / 100)] int page = 1,
        [FromQuery][Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var skip = (page - 1) * pageSize;
        var result = await promptService.GetPromptsPagedAsync(skip, pageSize, cancellationToken);
        return Ok(new PagedResponse<PromptDetails>(result.Items.Select(ToDto), result.TotalCount));
    }

    // Simple inline mapping; for more complex scenarios we would use AutoMapper or a dedicated mapping class.
    private static PromptDetails ToDto(Prompt p) =>
        new(p.Id, p.Text, p.Status, p.Response, p.CreatedAt, p.CompletedAt);
}
