namespace StackOverflowClone.Application.Questions;

public sealed record CreateQuestionRequest(string Title, string Content, IReadOnlyList<string> Tags);
