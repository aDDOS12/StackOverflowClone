using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowClone.Application.Comments;

namespace StackOverflowClone.Api.Controllers;

[ApiController]
public sealed class CommentsController(ICommentService commentService) : ControllerBase
{
    [Authorize]
    [HttpPost("api/questions/{questionId:int}/comments")]
    [ProducesResponseType(typeof(CreateCommentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateCommentResponse>> AddToQuestion(
        int questionId,
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await commentService.AddToQuestionAsync(questionId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created);
    }

    [Authorize]
    [HttpPost("api/questions/{questionId:int}/answers/{answerId:int}/comments")]
    [ProducesResponseType(typeof(CreateCommentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateCommentResponse>> AddToAnswer(
        int questionId,
        int answerId,
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await commentService.AddToAnswerAsync(questionId, answerId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet("api/questions/{questionId:int}/comments")]
    [ProducesResponseType(typeof(IReadOnlyList<CommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetForQuestion(
       int questionId,
       CancellationToken cancellationToken)
    {
        var response = await commentService.GetForQuestionAsync(questionId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("api/questions/{questionId:int}/answers/{answerId:int}/comments")]
    [ProducesResponseType(typeof(IReadOnlyList<CommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetForAnswer(
        int questionId,
        int answerId,
        CancellationToken cancellationToken)
    {
        var response = await commentService.GetForAnswerAsync(questionId, answerId, cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpDelete("api/comments/{commentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int commentId, CancellationToken cancellationToken)
    {
        await commentService.DeleteAsync(commentId, cancellationToken);
        return NoContent();
    }
}
