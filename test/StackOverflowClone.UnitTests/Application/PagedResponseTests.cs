using StackOverflowClone.Application.Common.Models;

namespace StackOverflowClone.UnitTests.Application;

public sealed class PagedResponseTests
{
    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(40, 20, 2)]
    [InlineData(41, 20, 3)]
    [InlineData(45, 20, 3)]
    public void TotalPages_RoundsUpToFullPages(int totalCount, int pageSize, int expectedPages)
    {
        var response = new PagedResponse<int>([], Page: 1, PageSize: pageSize, TotalCount: totalCount);

        Assert.Equal(expectedPages, response.TotalPages);
    }
}
