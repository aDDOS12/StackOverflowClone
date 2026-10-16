namespace StackOverflowClone.Application.Answers;

public interface IAnswerService
{
    Task<CreateAnswerResponse> CreateAsync(int questionId, CreateAnswerRequest request, CancellationToken cancellationToken);
}
