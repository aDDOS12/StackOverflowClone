namespace StackOverflowClone.Application.Votes;

public interface IVoteService
{
    Task VoteOnQuestionAsync(int questionId, VoteRequest request, CancellationToken cancellationToken);
    Task RemoveQuestionVoteAsync(int questionId, CancellationToken cancellationToken);
}
