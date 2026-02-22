using Microsoft.AspNetCore.Mvc;
using Prompter.Core.Entities;
using Prompter.Core.Services;
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
    public async Task<ActionResult<IEnumerable<PromptDetails>>> CreatePrompts([FromBody] CreatePromptsRequest request)
    {
        var prompts = await _promptService.CreatePromptsAsync(request.Prompts);
        return Ok(prompts.Select(ToDto));
    }

    [HttpGet]
    public async Task<IActionResult> GetPrompts([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        if (page.HasValue && pageSize.HasValue)
        {
            var skip = (page.Value - 1) * pageSize.Value;
            var (items, totalCount) = await _promptService.GetPromptsPagedAsync(skip, pageSize.Value);
            return Ok(new PagedResponse<PromptDetails>(items.Select(ToDto), totalCount));
        }

        var prompts = await _promptService.GetAllPromptsAsync();
        return Ok(prompts.Select(ToDto));
    }

    private static PromptDetails ToDto(Prompt p) =>
        new(p.Id, p.Text, p.Status, p.Response, p.CreatedAt, p.CompletedAt);
}
