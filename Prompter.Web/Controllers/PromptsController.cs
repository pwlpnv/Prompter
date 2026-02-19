using Microsoft.AspNetCore.Mvc;
using Prompter.Web.DTOs;

namespace Prompter.Web.Controllers;

public class PromptsController : ControllerBase
{
    [HttpPost]
    public ActionResult CreatePrompts([FromBody]CreatePromptsRequest request)
    {
        return Ok();
    }

    [HttpGet]
    public ActionResult<IEnumerable<PromptDetails>> GetPrompts()
    {
        return Ok();
    }
}