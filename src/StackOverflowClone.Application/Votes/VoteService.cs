using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Application.Common;
using StackOverflowClone.Application.Common.Exceptions;
using StackOverflowClone.Application.Common.Interfaces;
using StackOverflowClone.Domain.Entities;
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

        var oldValue = existingVote?.Value ?? 0;
        var delta = newValue - oldValue;

        if (delta == 0)
        {
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        if (existingVote is null)
        {
            context.QuestionVotes.Add(new QuestionVote
            {
                QuestionId = questionId,
                UserId = userId,
                Value = newValue
            });
        }
        else if (newValue == 0)
        {
            context.QuestionVotes.Remove(existingVote);
        }
        else
        {
            existingVote.Value = newValue;
        }

        await context.SaveChangesAsync(cancellationToken);

        await context.Questions.Where(q => q.Id == questionId).ExecuteUpdateAsync(s => s.SetProperty(q => q.Score, q => q.Score + delta), cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}
