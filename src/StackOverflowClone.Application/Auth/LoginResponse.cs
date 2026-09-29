namespace StackOverflowClone.Application.Auth;

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc);
