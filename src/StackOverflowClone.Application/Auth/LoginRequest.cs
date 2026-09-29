namespace StackOverflowClone.Application.Auth;

public sealed record LoginRequest(string Email, string Password)
{
    public override string ToString()
    {
        return $"{nameof(LoginRequest)} {{Email = {Email}, Password = *** }}";
    }
}
