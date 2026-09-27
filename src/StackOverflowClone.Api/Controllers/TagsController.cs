using Microsoft.AspNetCore.Mvc;
using StackOverflowClone.Application.Tags;

namespace StackOverflowClone.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TagsController(ITagService tagService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TagDto>>> GetAll(CancellationToken cancellationToken)
    {
        var tags = await tagService.GetAllAsync(cancellationToken);
        return Ok(tags);
    }
}
