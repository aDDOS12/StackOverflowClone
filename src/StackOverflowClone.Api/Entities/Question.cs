using StackOverflowClone.Api.Interfaces;

namespace StackOverflowClone.Api.Entities;

public class Question : IVoteable, ISoftDeletable, IHasTimestamps
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int Score { get; set; } = 0;
    public User User { get; set; } = default!;
    public Guid UserId { get; set; }
    public List<Answer> Answers { get; set; } = new List<Answer>();
    public List<Comment> Comments { get; set; } = new List<Comment>();
    public List<QuestionVote> QuestionVotes { get; set; } = new List<QuestionVote>();
    public List<Tag> Tags { get; set; } = new List<Tag>();
    public DateTime? DeletedAt { get; set; }
}
