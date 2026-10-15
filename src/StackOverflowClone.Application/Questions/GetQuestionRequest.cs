using FluentValidation;

namespace StackOverflowClone.Application.Questions;

public sealed class GetQuestionRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public QuestionSort Sort { get; init; } = QuestionSort.Newest;
    public string? Tag { get; init; }
}
