namespace StackOverflowClone.Application.Users;

public sealed record CurrentUserResponse(Guid Id, string Username, string Email, DateTime CreatedAtUtc);
