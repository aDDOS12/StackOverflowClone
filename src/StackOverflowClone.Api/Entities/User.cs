using StackOverflowClone.Api.Interfaces;

namespace StackOverflowClone.Api.Entities;

public class User : ISoftDeletable
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public DateTime AccountCreated { get; set; }
    //initialize empty list in order to avoid nullable variable
    public List<Question> Questions { get; set; } = new List<Question>();
    public List<Answer> Answers { get; set; } = new List<Answer>();
    public List<Comment> Comments { get; set; } = new List<Comment>();
    public List<QuestionVote> QuestionVotes { get; set; } = new List<QuestionVote>();
    public List<AnswerVote> AnswerVotes { get; set; } = new List<AnswerVote>();
    public List<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();
    public DateTime? DeletedAt { get; set; }
}