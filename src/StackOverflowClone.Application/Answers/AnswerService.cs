using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Application.Common;
using StackOverflowClone.Application.Common.Exceptions;
using StackOverflowClone.Application.Common.Interfaces;
using StackOverflowClone.Application.Common.Models;
using StackOverflowClone.Domain.Entities;
using ValidationException = StackOverflowClone.Application.Common.Exceptions.ValidationException;

namespace StackOverflowClone.Application.Answers;

public sealed class AnswerService(IApplicationDbContext context, IValidator<CreateAnswerRequest> createValidator, IValidator<GetAnswersRequest> getValidator, IValidator<UpdateAnswerRequest> updateValidator, ICurrentUser currentUser) : IAnswerService
{
    public async Task<CreateAnswerResponse> CreateAsync(int questionId, CreateAnswerRequest request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetActiveUserIdAsync(context, cancellationToken);

        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var questionExists = await context.Questions.AnyAsync(q => q.Id == questionId, cancellationToken);
        if (!questionExists)
        {
            await EnsureQuestionExistsAsync(questionId, cancellationToken);
        }

        var answer = new Answer
        {
            Content = request.Content,
            QuestionId = questionId,
            UserId = userId
        };

        context.Answers.Add(answer);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateAnswerResponse(answer.Id);
    }

    private async Task EnsureQuestionExistsAsync(int questionId, CancellationToken cancellationToken)
    {
        var questionExits = await context.Questions.AnyAsync(q => q.Id == questionId, cancellationToken);

        if (!questionExits)
        {
            throw new NotFoundException($"Question with id {questionId} was not found.");
        }
    }

    public async Task<PagedResponse<AnswerResponse>> GetByQuestionAsync(int questionId, GetAnswersRequest request, CancellationToken cancellationToken)
    {
        var validationReult = await getValidator.ValidateAsync(request, cancellationToken);
        if (!validationReult.IsValid)
        {
            throw new ValidationException(validationReult.Errors);
        }

        await EnsureQuestionExistsAsync(questionId, cancellationToken);

        var query = context.Answers.Where(a => a.QuestionId == questionId);

        query = request.Sort switch
        {
            AnswerSort.Newest => query.OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.Id),
            AnswerSort.Oldest => query.OrderBy(a => a.CreatedAt).ThenBy(a => a.Id),
            _ => query.OrderByDescending(a => a.Score).ThenBy(a => a.CreatedAt).ThenBy(a => a.Id)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AnswerResponse(
                a.Id,
                a.Content,
                a.Score,
                a.IsAccepted,
                a.CreatedAt,
                a.UpdatedAt,
                new AuthorDto(a.User.Id, a.User.Username)))
            .ToListAsync(cancellationToken);

        return new PagedResponse<AnswerResponse>(items, request.Page, request.PageSize, totalCount);
    }

    public async Task UpdateAsync(int questionId, int answerId, UpdateAnswerRequest request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetActiveUserIdAsync(context, cancellationToken);

        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var answer = await GetOwnedAnswerAsync(questionId, answerId, userId, cancellationToken);

        answer.Content = request.Content;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int questionId, int answerId, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetActiveUserIdAsync(context, cancellationToken);

        var answer = await GetOwnedAnswerAsync(questionId, answerId, userId, cancellationToken);

        context.Answers.Remove(answer);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Answer> GetOwnedAnswerAsync(int questionId, int answerId, Guid userId, CancellationToken cancellationToken)
    {
        await EnsureQuestionExistsAsync(questionId, cancellationToken);

        var answer = await context.Answers.SingleOrDefaultAsync(a => a.Id == answerId && a.QuestionId == questionId, cancellationToken)
            ?? throw new NotFoundException($"Answer with id {answerId} was not found.");

        if (answer.UserId != userId)
        {
            throw new ForbiddenException("Only the author can modify this answer.");
        }

        return answer;
    }
}
