using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowClone.Application.Answers;
using StackOverflowClone.Application.Common.Models;

namespace StackOverflowClone.Api.Controllers;

[ApiController]
[Route("api/questions/{questionId:int}/answers")]
public sealed class AnswersController(IAnswerService answerService) : ControllerBase
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

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<AnswerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<AnswerResponse>>> GetByQuestion(
        int questionId,
        [FromQuery] GetAnswersRequest request,
        CancellationToken cancellationToken)
    {
        var response = await answerService.GetByQuestionAsync(questionId, request, cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpPut("{answerId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int questionId,
        int answerId,
        UpdateAnswerRequest request,
        CancellationToken cancellationToken)
    {
        await answerService.UpdateAsync(questionId, answerId, request, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{answerId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int questionId,
        int answerId,
        CancellationToken cancellationToken)
    {
        await answerService.DeleteAsync(questionId, answerId, cancellationToken);
        return NoContent();
    }
}
