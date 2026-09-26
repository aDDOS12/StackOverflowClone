namespace StackOverflowClone.Domain.Entities;

public class AnswerVote : Vote
{
    //foreign key
    public int AnswerId { get; set; }
    //navigation property
    public Answer Answer { get; set; } = default!;
}
