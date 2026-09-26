namespace StackOverflowClone.Domain.Entities;

public class CommentVote : Vote
{
    //foreign key
    public int CommentId { get; set; }
    //navigation property
    public Comment Comment { get; set; } = default!;
}
