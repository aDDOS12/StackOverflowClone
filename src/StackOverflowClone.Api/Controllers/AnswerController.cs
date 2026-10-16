using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowClone.Application.Answers;

namespace StackOverflowClone.Api.Controllers;

[ApiController]
[Route("api/questions/{questionId:int}/answers")]
public sealed class AnswerController(IAnswerService answerService) : ControllerBase
{
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CreateAnswerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateAnswerResponse>> Create(
        int questionId,
        CreateAnswerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await answerService.CreateAsync(questionId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
