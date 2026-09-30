using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Application.Common.Exceptions;
using StackOverflowClone.Application.Common.Interfaces;
using StackOverflowClone.Domain.Entities;
using ValidationException = StackOverflowClone.Application.Common.Exceptions.ValidationException;

namespace StackOverflowClone.Application.Questions;

public sealed class QuestionService(IApplicationDbContext context, IValidator<CreateQuestionRequest> createValidator, ICurrentUser currentUser) : IQuestionService
{
    public async Task<CreateQuestionResponse> CreateAsync(CreateQuestionRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated");

        var authorExists = await context.Users.AnyAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);

        if (!authorExists)
        {
            throw new UnauthorizedException("User account no longer exists.");
        }

        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var tagNames = request.Tags
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        var tags = await context.Tags
            .Where(t => tagNames.Contains(t.TagName))
            .ToListAsync(cancellationToken);

        var unknownTags = tagNames.Except(tags.Select(t => t.TagName)).ToList();

        if (unknownTags.Count > 0)
        {
            throw new ValidationException(
                [
                    new ValidationFailure(nameof(request.Tags), $"Unknown tags: {string.Join(", ", unknownTags)}.")
                ]);
        }

        var question = new Question
        {
            Title = request.Title,
            Content = request.Content,
            UserId = userId,
            Tags = tags,
        };

        context.Questions.Add(question);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateQuestionResponse(question.Id);
    }
}