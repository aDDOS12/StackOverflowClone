namespace StackOverflowClone.Application.Tags;

public interface ITagService
{
    Task<IReadOnlyList<TagDto>> GetAllAsync(CancellationToken cancellationToken);
}
