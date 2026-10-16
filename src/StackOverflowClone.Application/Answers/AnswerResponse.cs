using StackOverflowClone.Application.Common.Models;

namespace StackOverflowClone.Application.Answers;

public sealed record AnswerResponse(int Id, string Content, int Score, bool IsAccepted, DateTime CreatedAtUtc, DateTime? UodatedAtUtc, AuthorDto Author);
