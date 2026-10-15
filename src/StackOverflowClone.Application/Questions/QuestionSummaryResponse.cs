using StackOverflowClone.Application.Common.Models;

namespace StackOverflowClone.Application.Questions;

public sealed record QuestionSummaryResponse(
    int Id,
    string Title,
    int Score,
    DateTime CreatedAtUtc,
    AuthorDto Author,
    IReadOnlyList<string> Tags,
    int AnswerCount);
