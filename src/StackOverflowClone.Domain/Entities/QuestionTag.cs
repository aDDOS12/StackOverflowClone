namespace StackOverflowClone.Domain.Entities;

public class QuestionTag
{
    public Question Question { get; set; } = default!;
    public int QuestionId { get; set; }
    public Tag Tag { get; set; } = default!;
    public int TagId { get; set; }
}
