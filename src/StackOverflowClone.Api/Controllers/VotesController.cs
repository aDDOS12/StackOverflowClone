using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowClone.Application.Votes;
using System.Runtime.CompilerServices;

namespace StackOverflowClone.Api.Controllers;

[ApiController]
[Authorize]
public sealed class VotesController(IVoteService voteService) : ControllerBase
{
    [HttpPut("api/questions/{questionId:int}/vote")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VoteOnQuestion(
        int questionId,
        VoteRequest request,
        CancellationToken cancellationToken)
    {
        await voteService.VoteOnQuestionAsync(questionId, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/questions/{questionId:int}/vote")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveQuestionVote(int questionId, CancellationToken cancellationToken)
    {
        await voteService.RemoveQuestionVoteAsync(questionId, cancellationToken);
        return NoContent();
    }
}
