using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Application.Common;
using StackOverflowClone.Application.Common.Exceptions;
using StackOverflowClone.Application.Common.Interfaces;
using ValidationException = StackOverflowClone.Application.Common.Exceptions.ValidationException;

namespace StackOverflowClone.Application.Auth;

public sealed class AuthService(IApplicationDbContext context, IValidator<LoginRequest> validator, IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator) : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid email or password";

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var email = EmailNormalizer.Normalize(request.Email);

        var user = await context.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == email && u.DeletedAt == null, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        var token = tokenGenerator.Generate(user);
        return new LoginResponse(token.Value, token.ExpiresAtUtc);
    }
}
