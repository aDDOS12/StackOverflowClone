namespace StackOverflowClone.Application.Questions;

public sealed record UpdateQuestionRequest(string Title, string Content, IReadOnlyList<string> Tags);
