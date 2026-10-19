using StackOverflowClone.Application.Auth;
using StackOverflowClone.Application.Users;
using StackOverflowClone.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace StackOverflowClone.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthEndpointsTests(ApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task RegisterThenLogin_ReturnsAccessToken()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var registerResponse = await Client.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest("jan_kowalski", "jan@example.com", "Haslo1234"),
            cancellationToken);

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("jan@example.com", "Haslo1234"),
            cancellationToken);

        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);

        Assert.False(string.IsNullOrEmpty(login?.AccessToken));
    }
}
