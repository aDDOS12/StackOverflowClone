using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Application.Common;
using StackOverflowClone.Application.Common.Exceptions;
using StackOverflowClone.Application.Common.Interfaces;
using StackOverflowClone.Domain.Entities;
using StackOverflowClone.Domain.Votes;
using ValidationException = StackOverflowClone.Application.Common.Exceptions.ValidationException;

namespace StackOverflowClone.Application.Votes;

public sealed class VoteService(IApplicationDbContext context, IValidator<VoteRequest> validator, ICurrentUser currentUser) : IVoteService
{
    public async Task VoteOnQuestionAsync(int questionId, VoteRequest request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetActiveUserIdAsync(context, cancellationToken);

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        await SetQuestionVoteAsync(questionId, userId, request.Value, cancellationToken);
    }

    public async Task RemoveQuestionVoteAsync(int questionId, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetActiveUserIdAsync(context, cancellationToken);
        await SetQuestionVoteAsync(questionId, userId, newValue: 0, cancellationToken);
    }

    private async Task SetQuestionVoteAsync(int questionId, Guid userId, int newValue, CancellationToken cancellationToken)
    {
        var authorId = await context.Questions
            .Where(q => q.Id == questionId)
            .Select(q => (Guid?)q.UserId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Question with id {questionId} was not found.");

        if (authorId == userId)
        {
            throw new ForbiddenException("You cannot vote on your own question.");
        }

        var existingVote = await context.QuestionVotes
            .SingleOrDefaultAsync(v => v.QuestionId == questionId && v.UserId == userId, cancellationToken);

        var change = VoteChange.Calculate(existingVote?.Value, newValue);

        if (change.Action == VoteAction.None)
        {
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        switch (change.Action)
        {
            case VoteAction.Add:
                context.QuestionVotes.Add(new QuestionVote
                {
                    QuestionId = questionId,
                    UserId = userId,
                    Value = newValue
                });
                break;
            case VoteAction.Remove:
                context.QuestionVotes.Remove(existingVote!);
                break;
            case VoteAction.Update:
                existingVote!.Value = newValue;
                break;
        }

        await context.SaveChangesAsync(cancellationToken);

        await context.Questions.Where(q => q.Id == questionId).ExecuteUpdateAsync(s => s.SetProperty(q => q.Score, q => q.Score + change.ScoreDelta), cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task VoteOnAnswerAsync(int questionId, int answerId, VoteRequest request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetActiveUserIdAsync(context, cancellationToken);

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        await SetAnswerVoteAsync(questionId, answerId, userId, request.Value, cancellationToken);
    }

    public async Task RemoveAnswerVoteAsync(int questionId, int answerId, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetActiveUserIdAsync(context, cancellationToken);
        await SetAnswerVoteAsync(questionId, answerId, userId, newValue: 0, cancellationToken);
    }

    private async Task SetAnswerVoteAsync(int questionId, int answerId, Guid userId, int newValue, CancellationToken cancellationToken)
    {
        await context.EnsureQuestionExistsAsync(questionId, cancellationToken);

        var authorId = await context.Answers
            .Where(a => a.Id == answerId && a.QuestionId == questionId)
            .Select(a => (Guid?)a.UserId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Answer with id {answerId} was not found.");

        if (authorId == userId)
        {
            throw new ForbiddenException("You cannot vote on your own answer.");
        }

        var existingVote = await context.AnswerVotes
        .SingleOrDefaultAsync(v => v.AnswerId == answerId && v.UserId == userId, cancellationToken);

        var change = VoteChange.Calculate(existingVote?.Value, newValue);

        if (change.Action == VoteAction.None)
        {
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        switch (change.Action)
        {
            case VoteAction.Add:
                context.AnswerVotes.Add(new AnswerVote
                {
                    AnswerId = answerId,
                    UserId = userId,
                    Value = newValue
                });
                break;
            case VoteAction.Remove:
                context.AnswerVotes.Remove(existingVote!);
                break;
            case VoteAction.Update:
                existingVote!.Value = newValue;
                break;
        }

        await context.SaveChangesAsync(cancellationToken);

        await context.Answers
            .Where(a => a.Id == answerId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Score, a => a.Score + change.ScoreDelta), cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}
