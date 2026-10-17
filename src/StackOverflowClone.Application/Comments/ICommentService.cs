namespace StackOverflowClone.Application.Comments;

public interface ICommentService
{
    Task<CreateCommentResponse> AddToQuestionAsync(int questionId, CreateCommentRequest request, CancellationToken cancellationToken);
    Task<CreateCommentResponse> AddToAnswerAsync(int questionId, int answerId, CreateCommentRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<CommentResponse>> GetForQuestionAsync(int questionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CommentResponse>> GetForAnswerAsync(int questionId, int answerId, CancellationToken cancellationToken);
    Task DeleteAsync(int commentId, CancellationToken cancellationToken);
}
