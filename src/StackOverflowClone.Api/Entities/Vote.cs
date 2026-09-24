namespace StackOverflowClone.Api.Entities;

public abstract class Vote
{
    //foreign key
    public Guid UserId { get; set; }
    //navigation property
    public User User { get; set; } = default!;
    public int Value { get; set; }
}
