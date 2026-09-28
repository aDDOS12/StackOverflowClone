namespace StackOverflowClone.Application.Users;

public sealed record RegisterUserRequest(string Username, string Email, string Password)
{
    public override string ToString()
    {
        return $"{nameof(RegisterUserRequest)} {{Username = {Username}, Email = {Email}, Password = *** }}";
    }
}
