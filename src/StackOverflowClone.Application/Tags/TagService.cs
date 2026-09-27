using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Application.Common.Interfaces;

namespace StackOverflowClone.Application.Tags;

public sealed class TagService(IApplicationDbContext context) : ITagService
{
    public async Task<IReadOnlyList<TagDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Tags
            .OrderBy(t => t.TagName)
            .Select(t => new TagDto(t.Id, t.TagName))
            .ToListAsync(cancellationToken);
    }
}
