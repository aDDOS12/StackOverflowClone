using StackOverflowClone.Api.Interfaces;

namespace StackOverflowClone.Api.Entities;

public class Comment : IVoteable, ISoftDeletable, IHasTimestamps
{
    public int Id { get; set; }
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int Score { get; set; }
    public User User { get; set; } = default!;
    public Guid UserId { get; set; }
    public Question? Question { get; set; }
    public int? QuestionId { get; set; }
    public Answer? Answer { get; set; }
    public int? AnswerId { get; set; }
    public List<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();
    public DateTime? DeletedAt { get; set; }
}
