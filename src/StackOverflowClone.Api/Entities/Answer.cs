using StackOverflowClone.Api.Interfaces;

namespace StackOverflowClone.Api.Entities;

public class Answer : IVoteable, ISoftDeletable, IHasTimestamps
{
    public int Id { get; set; }
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsAccepted { get; set; } = false;
    public int Score { get; set; } = 0;
    public User User { get; set; } = default!;
    public Guid UserId { get; set; }
    public Question Question { get; set; } = default!;
    public int QuestionId { get; set; }
    public List<Comment> Comments { get; set; } = new List<Comment>();
    public List<AnswerVote> AnswerVotes { get; set; } = new List<AnswerVote>();
    public DateTime? DeletedAt { get; set; }
}
