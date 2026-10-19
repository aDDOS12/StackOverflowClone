using StackOverflowClone.Application.Tags;
using StackOverflowClone.IntegrationTests.Infrastructure;
using System.Net.Http.Json;

namespace StackOverflowClone.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class TagsEndpointTests(ApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetTags_ReturnAllSeededTags()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var response = await Client.GetAsync("/api/tags", cancellationToken);

        response.EnsureSuccessStatusCode();
        var tags = await response.Content.ReadFromJsonAsync<List<TagDto>>(cancellationToken);

        Assert.NotNull(tags);
        Assert.Equal(52, tags.Count);
        Assert.Contains(tags, tag => tag.Name == "c#");
    }
}
