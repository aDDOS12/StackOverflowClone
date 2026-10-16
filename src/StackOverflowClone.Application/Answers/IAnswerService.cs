using StackOverflowClone.Application.Common.Models;

namespace StackOverflowClone.Application.Answers;

public interface IAnswerService
{
    Task<CreateAnswerResponse> CreateAsync(int questionId, CreateAnswerRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<AnswerResponse>> GetByQuestionAsync(int questionid, GetAnswersRequest request, CancellationToken cancellationToken);
}
