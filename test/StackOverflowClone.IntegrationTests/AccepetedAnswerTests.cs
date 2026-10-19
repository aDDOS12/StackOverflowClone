using Microsoft.AspNetCore.Mvc;
using StackOverflowClone.Application.Questions;
using StackOverflowClone.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace StackOverflowClone.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AccepetedAnswerTests(ApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AcceptingAnotherAnswer_UnacceptsThePreviousOne()
    {
        var author = await CreateAuthenticatedClientAsync("author");
        var firstAnswerer = await CreateAuthenticatedClientAsync("first");
        var secondAnswerer = await CreateAuthenticatedClientAsync("second");
        var questionId = await CreateQuestionAsync(author);
        var firstAnswerId = await CreateAnswerAsync(firstAnswerer, questionId);
        var secondAnswerId = await CreateAnswerAsync(secondAnswerer, questionId);

        await AcceptAsync(author, questionId, firstAnswerId);
        await AcceptAsync(author, questionId, secondAnswerId);

        var answers = await GetAnswersAsync(questionId);
        var accepted = Assert.Single(answers, answer => answer.IsAccepted);
        Assert.Equal(secondAnswerId, accepted.Id);
    }

    [Fact]
    public async Task AcceptingAnswer_ByNonAuthor_ReturnsForbidden()
    {
        var author = await CreateAuthenticatedClientAsync("author");
        var answerer = await CreateAuthenticatedClientAsync("answerer");
        var questionId = await CreateQuestionAsync(author);
        var answerId = await CreateAnswerAsync(answerer, questionId);

        var response = await answerer.PutAsJsonAsync(
            $"/api/questions/{questionId}/accepted-answer", new AcceptAnswerRequest(answerId), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeletingQuestionWithAcceptedAnswer_ReturnsConflictWithErrorCode()
    {
        var author = await CreateAuthenticatedClientAsync("author");
        var answerer = await CreateAuthenticatedClientAsync("answerer");
        var questionId = await CreateQuestionAsync(author);
        var answerId = await CreateAnswerAsync(answerer, questionId);
        await AcceptAsync(author, questionId, answerId);

        var response = await author.DeleteAsync($"/api/questions/{questionId}", CancellationToken);

        await AssertConflictAsync(response, "question_has_valuable_answers");
    }

    [Fact]
    public async Task DeletingAcceptedAnswer_ByItsAuthor_ReturnsConflictWithErrorCode()
    {
        var author = await CreateAuthenticatedClientAsync("author");
        var answerer = await CreateAuthenticatedClientAsync("answerer");
        var questionId = await CreateQuestionAsync(author);
        var answerId = await CreateAnswerAsync(answerer, questionId);
        await AcceptAsync(author, questionId, answerId);

        var response = await answerer.DeleteAsync(
            $"/api/questions/{questionId}/answers/{answerId}", CancellationToken);

        await AssertConflictAsync(response, "accepted_answer_cannot_be_deleted");
    }

    private static async Task AcceptAsync(HttpClient client, int questionId, int answerId)
    {
        var response = await client.PutAsJsonAsync(
            $"/api/questions/{questionId}/accepted-answer", new AcceptAnswerRequest(answerId), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static async Task AssertConflictAsync(HttpResponseMessage response, string expectedCode)
    {
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(CancellationToken);
        Assert.NotNull(problem);
        Assert.Equal(expectedCode, problem.Extensions["code"]?.ToString());
    }
}
