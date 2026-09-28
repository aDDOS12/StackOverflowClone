using Microsoft.AspNetCore.Identity;
using StackOverflowClone.Application.Common.Interfaces;
using StackOverflowClone.Domain.Entities;

namespace StackOverflowClone.Infrastructure.Security;

public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(null!, password);
    }

    public bool Verify(string password, string passwordHash)
    {
        return _hasher.VerifyHashedPassword(null!, passwordHash, password) != PasswordVerificationResult.Failed;
    }
}
