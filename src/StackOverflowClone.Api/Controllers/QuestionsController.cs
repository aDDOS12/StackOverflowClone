using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowClone.Application.Common.Models;
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
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(QuestionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionDetailResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await questionService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<QuestionSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<QuestionSummaryResponse>>> Get(
        [FromQuery] GetQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await questionService.GetAsync(request, cancellationToken);
        return Ok(response);
    }
}
