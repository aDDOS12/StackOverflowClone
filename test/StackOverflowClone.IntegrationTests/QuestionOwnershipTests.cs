using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackOverflowClone.Application.Common;
using StackOverflowClone.Application.Common.Models;
using StackOverflowClone.Application.Questions;
using StackOverflowClone.Infrastructure.Persistence;
using StackOverflowClone.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace StackOverflowClone.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class QuestionOwnershipTests(ApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task UpdatingSomeoneElsesQuestion_ReturnsForbidden()
    {
        var author = await CreateAuthenticatedClientAsync("author");
        var otherUser = await CreateAuthenticatedClientAsync("other");
        var questionId = await CreateQuestionAsync(author);

        var response = await otherUser.PutAsJsonAsync(
            $"/api/questions/{questionId}",
            new UpdateQuestionRequest(
                "A completely different title",
                "Someone else is trying to overwrite this question's content here.",
                ["c#"]),
            CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeletingQuestion_HidesItButKeepsItInDatabase()
    {
        var author = await CreateAuthenticatedClientAsync("author");
        var questionId = await CreateQuestionAsync(author);

        var deleteResponse = await author.DeleteAsync($"/api/questions/{questionId}", CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var detailsResponse = await Client.GetAsync($"/api/questions/{questionId}", CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, detailsResponse.StatusCode);

        var list = await Client.GetFromJsonAsync<PagedResponse<QuestionSummaryResponse>>(
            "/api/questions", CancellationToken);
        Assert.DoesNotContain(list!.Items, question => question.Id == questionId);

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StackOverflowContext>();

        var storedQuestion = await context.Questions
            .IgnoreQueryFilters([QueryFilterNames.SoftDelete])
            .SingleAsync(q => q.Id == questionId, CancellationToken);

        Assert.NotNull(storedQuestion.DeletedAt);
    }
}
