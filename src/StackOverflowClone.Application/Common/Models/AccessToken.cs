namespace StackOverflowClone.Application.Common.Models;

public sealed record AccessToken(string Value, DateTime ExpiresAtUtc);
