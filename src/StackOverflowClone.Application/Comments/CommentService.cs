using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Application.Common;
using StackOverflowClone.Application.Common.Exceptions;
using StackOverflowClone.Application.Common.Interfaces;
using StackOverflowClone.Application.Common.Models;
using StackOverflowClone.Domain.Entities;
using ValidationException = StackOverflowClone.Application.Common.Exceptions.ValidationException;

namespace StackOverflowClone.Application.Comments;

public sealed class CommentService(IApplicationDbContext context, IValidator<CreateCommentRequest> createValidator, ICurrentUser currentUser) : ICommentService
{
    public async Task<CreateCommentResponse> AddToQuestionAsync(int questionId, CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetActiveUserIdAsync(context, cancellationToken);
        await ValidateAsync(request, cancellationToken);
        await context.EnsureQuestionExistsAsync(questionId, cancellationToken);

        var comment = new Comment
        {
            Content = request.Content,
            QuestionId = questionId,
            UserId = userId
        };

        return await SaveAsync(comment, cancellationToken);
    }

    public async Task<CreateCommentResponse> AddToAnswerAsync(int questionId, int answerId, CreateCommentRequest request, CancellationToken cancellationToken) 
    {
        var userId = await currentUser.GetActiveUserIdAsync(context, cancellationToken);
        await ValidateAsync(request, cancellationToken);
        await context.EnsureAnswerExistsAsync(questionId, answerId, cancellationToken);

        var comment = new Comment
        {
            Content = request.Content,
            AnswerId = answerId,
            UserId = userId
        };

        return await SaveAsync(comment, cancellationToken);
    }

    public async Task<IReadOnlyList<CommentResponse>> GetForQuestionAsync(int questionId, CancellationToken cancellationToken)
    {
        await context.EnsureQuestionExistsAsync(questionId, cancellationToken);
        return await ToResponsesAsync(context.Comments.Where(c => c.QuestionId == questionId), cancellationToken);
    }

    public async Task<IReadOnlyList<CommentResponse>> GetForAnswerAsync(int questionId, int answerId, CancellationToken cancellationToken)
    {
        await context.EnsureAnswerExistsAsync(questionId, answerId, cancellationToken);
        return await ToResponsesAsync(context.Comments.Where(c => c.AnswerId == answerId), cancellationToken);
    }

    private async Task ValidateAsync(CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }

    private async Task<CreateCommentResponse> SaveAsync(Comment comment, CancellationToken cancellationToken)
    {
        context.Comments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);
        return new CreateCommentResponse(comment.Id);
    }

    private static async Task<IReadOnlyList<CommentResponse>> ToResponsesAsync(IQueryable<Comment> query, CancellationToken cancellationToken)
        => await query
        .OrderBy(c => c.CreatedAt)
        .ThenBy(c => c.Id)
        .Select(c => new CommentResponse(
            c.Id,
            c.Content,
            c.Score,
            c.CreatedAt,
            new AuthorDto(c.User.Id, c.User.Username)))
        .ToListAsync(cancellationToken);

    public async Task DeleteAsync(int commentId, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetActiveUserIdAsync(context, cancellationToken);

        var comment = await context.Comments.SingleOrDefaultAsync(c => c.Id == commentId, cancellationToken)
            ?? throw new NotFoundException($"Comment with id {commentId} was not found.");

        if (comment.UserId != userId)
        {
            throw new ForbiddenException("Only the author can delete this comment.");
        }

        context.Comments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken);
    }
}
