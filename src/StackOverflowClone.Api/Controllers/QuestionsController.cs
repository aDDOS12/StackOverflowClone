using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowClone.Application.Questions;

namespace StackOverflowClone.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class QuestionsController(IQuestionService questionService) : ControllerBase
{
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CreateQuestionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateQuestionResponse>> Create(CreateQuestionRequest request, CancellationToken cancellationToken)
    {
        var response = await questionService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
