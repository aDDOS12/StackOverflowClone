namespace StackOverflowClone.Application.Users;

public sealed record DeleteAccountRequest(string Password)
{
    public override string ToString() => $"{nameof(DeleteAccountRequest)} {{ Password = *** }}";
}
