namespace StackOverflowClone.Api.Interfaces;

public interface IHasTimestamps
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
