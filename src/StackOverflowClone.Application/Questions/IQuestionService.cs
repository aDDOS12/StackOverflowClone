using StackOverflowClone.Application.Common.Models;

namespace StackOverflowClone.Application.Questions;

public interface IQuestionService
{
    Task<CreateQuestionResponse> CreateAsync(CreateQuestionRequest request, CancellationToken cancellationToken);
    Task<QuestionDetailResponse> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<PagedResponse<QuestionSummaryResponse>> GetAsync(GetQuestionRequest request, CancellationToken cancellationToken);
    Task UpdateAsync(int id, UpdateQuestionRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
