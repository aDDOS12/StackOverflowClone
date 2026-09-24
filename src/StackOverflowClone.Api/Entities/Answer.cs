using StackOverflowClone.Api.Interfaces;

namespace StackOverflowClone.Api.Entities;

public class Answer : IVoteable
{
    public int Id { get; set; }
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsAccepted { get; set; }
    public int Score { get; set; }
    public User User { get; set; }
    public Guid UserId { get; set; }
    public Question Question { get; set; } = default!;
    public int QuestionId { get; set; }
    public List<Comment> Comments { get; set; } = new List<Comment>();
    public List<AnswerVote> AnswerVotes { get; set; } = new List<AnswerVote>();
}
