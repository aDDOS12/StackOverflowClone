using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using StackOverflowClone.Domain.Entities;
using StackOverflowClone.Infrastructure.Authentication;

namespace StackOverflowClone.UnitTests.Infrastructure;

public sealed class JwtTokenGeneratorTests
{
    private static readonly JwtOptions JwtSettings = new()
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        SigningKey = "test-signing-key-that-is-at-least-32-characters-long",
        ExpirationMinutes = 60
    };

    private static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Generate_SetsExpirationRelativeToCurrentTime()
    {
        var generator = CreateGenerator();

        var token = generator.Generate(CreateUser());

        Assert.Equal(Now.UtcDateTime.AddMinutes(60), token.ExpiresAtUtc);
    }

    [Fact]
    public void Generate_TokenContainUserIdAndUsername()
    {
        var user = CreateUser();
        var generator = CreateGenerator();

        var token = generator.Generate(user);
        var jwt = new JsonWebToken(token.Value);

        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Equal(user.Username, jwt.GetClaim(JwtRegisteredClaimNames.Name).Value);
        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Contains("test-audience", jwt.Audiences);
    }

    [Fact]
    public void Generate_TokenDoesNotContainEmail()
    {
        var user = CreateUser();
        var generator = CreateGenerator();

        var token = generator.Generate(user);
        var jwt = new JsonWebToken(token.Value);

        Assert.DoesNotContain(jwt.Claims, claim => claim.Value == user.Email);
    }

    private static JwtTokenGenerator CreateGenerator() =>
        new(Options.Create(JwtSettings), new FakeTimeProvider(Now));

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        Username = "jan_kowalski",
        Email = "jan@example.com",
        PasswordHash = "irrelevant"
    };
}
