using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Application.Common.Exceptions;
using StackOverflowClone.Application.Common.Interfaces;

namespace StackOverflowClone.Application.Common;

internal static class ApplicationDbContextExtensions
{
    public static async Task EnsureQuestionExistsAsync(this IApplicationDbContext context, int questionId, CancellationToken cancellationToken)
    {
        var exists = await context.Questions.AnyAsync(q => q.Id == questionId, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException($"Question with id {questionId} was not found.");
        }
    }

    public static async Task EnsureAnswerExistsAsync(this IApplicationDbContext context, int questionId, int answerId, CancellationToken cancellationToken)
    {
        await context.EnsureQuestionExistsAsync(questionId, cancellationToken);

        var exists = await context.Answers.AnyAsync(a => a.Id == answerId && a.QuestionId == questionId, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException($"Answer with id {answerId} was not found.");
        }
    }
}
