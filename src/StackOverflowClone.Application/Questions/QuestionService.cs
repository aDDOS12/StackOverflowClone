using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Application.Common;
using StackOverflowClone.Application.Common.Exceptions;
using StackOverflowClone.Application.Common.Interfaces;
using StackOverflowClone.Application.Common.Models;
using StackOverflowClone.Domain.Entities;
using ValidationException = StackOverflowClone.Application.Common.Exceptions.ValidationException;

namespace StackOverflowClone.Application.Questions;

public sealed class QuestionService(IApplicationDbContext context, IValidator<CreateQuestionRequest> createValidator, IValidator<GetQuestionRequest> getValidator, IValidator<UpdateQuestionRequest> updateValidator, ICurrentUser currentUser) : IQuestionService
{
    public async Task<CreateQuestionResponse> CreateAsync(CreateQuestionRequest request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetActiveUserIdAsync(context, cancellationToken);

        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var tags = await ResolveTagsAsync(request.Tags, cancellationToken);

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

    public async Task<QuestionDetailResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var question = await context.Questions
            .Where(q => q.Id == id)
            .Select(q => new QuestionDetailResponse(
                q.Id,
                q.Title,
                q.Content,
                q.Score,
                q.CreatedAt,
                q.UpdatedAt,
                new AuthorDto(q.User.Id, q.User.Username),
                q.Tags.OrderBy(t => t.TagName).Select(t => t.TagName).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return question ?? throw new NotFoundException($"Question with id {id} was not found.");
    }

    public async Task<PagedResponse<QuestionSummaryResponse>> GetAsync(GetQuestionRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await getValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        IQueryable<Question> query = context.Questions;

        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            var tagName = request.Tag.Trim().ToLowerInvariant();
            query = query.Where(q => q.Tags.Any(t => t.TagName == tagName));
        }

        query = request.Sort switch
        {
            QuestionSort.Score => query.OrderByDescending(q => q.Score).ThenByDescending(q => q.Id),
            _ => query.OrderByDescending(q => q.CreatedAt).ThenByDescending(q => q.Id)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(q => new QuestionSummaryResponse(
                q.Id,
                q.Title,
                q.Score,
                q.CreatedAt,
                new AuthorDto(q.User.Id, q.User.Username),
                q.Tags.OrderBy(t => t.TagName).Select(t => t.TagName).ToList(),
                q.Answers.Count))
            .ToListAsync(cancellationToken);

        return new PagedResponse<QuestionSummaryResponse>(items, request.Page, request.PageSize, totalCount);
    }

    private async Task<List<Tag>> ResolveTagsAsync(IEnumerable<string> requestedTags, CancellationToken cancellationToken)
    {
        var tagNames = requestedTags
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
                    new ValidationFailure(nameof(CreateQuestionRequest.Tags), $"Unknown tags: {string.Join(", ", unknownTags)}.")
                ]);
        }

        return tags;
    }

    public async Task UpdateAsync(int id, UpdateQuestionRequest request, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetActiveUserIdAsync(context, cancellationToken);

        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var question = await context.Questions
            .Include(q => q.Tags)
            .SingleOrDefaultAsync(q => q.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Question with id {id} was not found.");

        if (question.UserId != userId)
        {
            throw new ForbiddenException("Only the author can edit this question.");
        }

        var tags = await ResolveTagsAsync(request.Tags, cancellationToken);

        question.Title = request.Title;
        question.Content = request.Content;
        question.Tags.Clear();
        question.Tags.AddRange(tags);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetActiveUserIdAsync(context, cancellationToken);

        var question = await context.Questions.SingleOrDefaultAsync(q => q.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Question with id {id} was not found");

        if (question.UserId != userId)
        {
            throw new ForbiddenException("Only the author can delete this question");
        }

        context.Questions.Remove(question);
        await context.SaveChangesAsync(cancellationToken);
    }
}