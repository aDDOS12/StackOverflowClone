using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Application.Common.Exceptions;
using StackOverflowClone.Application.Common.Interfaces;
using StackOverflowClone.Domain.Entities;
using ValidationException = StackOverflowClone.Application.Common.Exceptions.ValidationException;

namespace StackOverflowClone.Application.Users;

public sealed class UserService(IApplicationDbContext context, IValidator<RegisterUserRequest> validator, IPasswordHasher passwordHasher) : IUserService
{
    public async Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var email = request.Email.Trim().ToLowerInvariant();

        if (await context.Users.AnyAsync(u => u.Username == request.Username, cancellationToken))
        {
            throw new ConflictException($"Username '{request.Username}' is already taken.");
        }

        if (await context.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            throw new ConflictException("There's already registered account on this email.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return new RegisterUserResponse(user.Id, user.Username, user.Email);
    }
}
