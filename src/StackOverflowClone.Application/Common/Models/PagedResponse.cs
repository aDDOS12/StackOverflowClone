namespace StackOverflowClone.Application.Common.Models;

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
