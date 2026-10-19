using StackOverflowClone.Application.Answers;
using StackOverflowClone.Application.Auth;
using StackOverflowClone.Application.Common.Models;
using StackOverflowClone.Application.Questions;
using StackOverflowClone.Application.Users;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StackOverflowClone.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase(ApiFactory factory) : IAsyncLifetime
{
    private const string Password = "Haslo1234";
    private readonly List<HttpClient> _authenticatedClients = [];

    protected ApiFactory Factory { get; } = factory;
    protected HttpClient Client { get; } = factory.CreateClient();
    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        
        foreach (var client in _authenticatedClients)
        {
            client.Dispose();
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    protected async Task<HttpClient> CreateAuthenticatedClientAsync(string username)
    {
        var email = $"{username}@example.com";

        var registerResponse = await Client.PostAsJsonAsync(
            "/api/users", new RegisterUserRequest(username, email, Password), CancellationToken);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await Client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest(email, Password), CancellationToken);
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(CancellationToken);

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        _authenticatedClients.Add(client);

        return client;
    }

    protected static async Task<int> CreateQuestionAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/questions",
            new CreateQuestionRequest(
                "How to filter a list with LINQ?",
                "I have a list of numbers and want to keep only the even ones. What is the idiomatic way?",
                ["c#", "linq"]),
            CancellationToken);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateQuestionResponse>(CancellationToken);
        return created!.Id;
    }

    protected async Task<QuestionDetailResponse> GetQuestionAsync(int questionId)
    {
        var question = await Client.GetFromJsonAsync<QuestionDetailResponse>(
            $"/api/questions/{questionId}", CancellationToken);

        return question!;
    }

    protected static async Task<int> CreateAnswerAsync(HttpClient client, int questionId)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/questions/{questionId}/answers",
            new CreateAnswerRequest("You can use the Where method with a lambda that checks if a number is even."),
            CancellationToken);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateAnswerResponse>(CancellationToken);
        return created!.Id;
    }

    protected async Task<IReadOnlyList<AnswerResponse>> GetAnswersAsync(int questionId)
    {
        var page = await Client.GetFromJsonAsync<PagedResponse<AnswerResponse>>(
            $"/api/questions/{questionId}/answers", CancellationToken);

        return page!.Items;
    }
}
