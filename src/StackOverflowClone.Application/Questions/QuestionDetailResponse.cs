using StackOverflowClone.Application.Common.Models;

namespace StackOverflowClone.Application.Questions;

public sealed record QuestionDetailResponse(
    int Id,
    string Title,
    string Content,
    int Score,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    AuthorDto Author,
    IReadOnlyList<string> Tags);
