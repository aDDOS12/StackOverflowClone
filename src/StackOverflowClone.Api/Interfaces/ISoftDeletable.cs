namespace StackOverflowClone.Api.Interfaces
{
    public interface ISoftDeletable
    {
        DateTime? DeletedAt { get; set; }
    }
}
