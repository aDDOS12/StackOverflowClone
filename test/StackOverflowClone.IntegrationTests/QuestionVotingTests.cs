using StackOverflowClone.Application.Votes;
using StackOverflowClone.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace StackOverflowClone.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class QuestionVotingTests(ApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Upvote_IncreasesScore_AndRepeatingItChangesNothing()
    {
        var author = await CreateAuthenticatedClientAsync("author");
        var voter = await CreateAuthenticatedClientAsync("voter");
        var questionId = await CreateQuestionAsync(author);

        await VoteAsync(voter, questionId, 1);
        await VoteAsync(voter, questionId, 1);

        var question = await GetQuestionAsync(questionId);
        Assert.Equal(1, question.Score);
    }

    [Fact]
    public async Task ChangingVoteFromUpToDown_DecreasesScoreByTwo()
    {
        var author = await CreateAuthenticatedClientAsync("author");
        var voter = await CreateAuthenticatedClientAsync("voter");
        var questionId = await CreateQuestionAsync(author);

        await VoteAsync(voter, questionId, 1);
        await VoteAsync(voter, questionId, -1);

        var question = await GetQuestionAsync(questionId);
        Assert.Equal(-1, question.Score);
    }

    [Fact]
    public async Task RemovingVote_RestoresScore()
    {
        var author = await CreateAuthenticatedClientAsync("author");
        var voter = await CreateAuthenticatedClientAsync("voter");
        var questionId = await CreateQuestionAsync(author);

        await VoteAsync(voter, questionId, 1);
        var response = await voter.DeleteAsync($"/api/questions/{questionId}/vote", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var question = await GetQuestionAsync(questionId);
        Assert.Equal(0, question.Score);
    }

    [Fact]
    public async Task VotingOnOwnQuestion_ReturnsForbidden()
    {
        var author = await CreateAuthenticatedClientAsync("author");
        var questionId = await CreateQuestionAsync(author);

        var response = await author.PutAsJsonAsync(
            $"/api/questions/{questionId}/vote", new VoteRequest(1), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Voting_DoesNotMarkQuestionAsEdited()
    {
        var author = await CreateAuthenticatedClientAsync("author");
        var voter = await CreateAuthenticatedClientAsync("voter");
        var questionId = await CreateQuestionAsync(author);

        await VoteAsync(voter, questionId, 1);

        var question = await GetQuestionAsync(questionId);
        Assert.Null(question.UpdatedAtUtc);
    }

    [Fact]
    public async Task ConcurrentVotes_AreAllCounted()
    {
        var author = await CreateAuthenticatedClientAsync("author");
        var questionId = await CreateQuestionAsync(author);

        var voters = new List<HttpClient>();
        for (var i = 0; i < 10; i++)
        {
            voters.Add(await CreateAuthenticatedClientAsync($"voter{i}"));
        }

        await Task.WhenAll(voters.Select(voter => VoteAsync(voter, questionId, 1)));

        var question = await GetQuestionAsync(questionId);
        Assert.Equal(10, question.Score);
    }

    private static async Task VoteAsync(HttpClient client, int questionId, int value)
    {
        var response = await client.PutAsJsonAsync(
            $"/api/questions/{questionId}/vote", new VoteRequest(value), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

}
