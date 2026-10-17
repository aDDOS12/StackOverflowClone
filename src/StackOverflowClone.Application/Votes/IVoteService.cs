namespace StackOverflowClone.Application.Votes;

public interface IVoteService
{
    Task VoteOnQuestionAsync(int questionId, VoteRequest request, CancellationToken cancellationToken);
    Task RemoveQuestionVoteAsync(int questionId, CancellationToken cancellationToken);
    Task VoteOnAnswerAsync(int questionId, int answerId, VoteRequest request, CancellationToken cancellationToken);
    Task RemoveAnswerVoteAsync(int questionId, int answerId, CancellationToken cancellationToken);
}
