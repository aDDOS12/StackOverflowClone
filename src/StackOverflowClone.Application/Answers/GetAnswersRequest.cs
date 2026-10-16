namespace StackOverflowClone.Application.Answers;

public sealed record GetAnswersRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 30;
    public AnswerSort Sort { get; init; } = AnswerSort.Score;
}
