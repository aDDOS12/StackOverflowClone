namespace StackOverflowClone.Application.Questions;

public interface IQuestionService
{
    Task<CreateQuestionResponse> CreateAsync(CreateQuestionRequest request, CancellationToken cancellationToken);
}
