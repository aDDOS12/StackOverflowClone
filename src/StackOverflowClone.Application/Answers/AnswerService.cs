using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Application.Common;
using StackOverflowClone.Application.Common.Exceptions;
using StackOverflowClone.Application.Common.Interfaces;
using StackOverflowClone.Domain.Entities;
using ValidationException = StackOverflowClone.Application.Common.Exceptions.ValidationException;

namespace StackOverflowClone.Application.Answers;

public sealed class AnswerService(IApplicationDbContext context, IValidator<CreateAnswerRequest> createValidator, ICurrentUser currentUser) : IAnswerService
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
            throw new NotFoundException($"Question with id {questionId} was not found.");
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
}
