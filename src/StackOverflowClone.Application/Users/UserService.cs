using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Application.Common;
using StackOverflowClone.Application.Common.Exceptions;
using StackOverflowClone.Application.Common.Interfaces;
using StackOverflowClone.Domain.Entities;
using System.Buffers.Text;
using ValidationException = StackOverflowClone.Application.Common.Exceptions.ValidationException;

namespace StackOverflowClone.Application.Users;

public sealed class UserService(IApplicationDbContext context, IValidator<RegisterUserRequest> registerValidator, IValidator<DeleteAccountRequest> deleteValidator, IPasswordHasher passwordHasher) : IUserService
{
    public async Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var email = EmailNormalizer.Normalize(request.Email);

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

    public async Task<CurrentUserResponse> GetCurrentAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Where(u => u.Id == userId && u.DeletedAt == null)
            .Select(u => new CurrentUserResponse(u.Id, u.Username, u.Email, u.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return user ?? throw new NotFoundException("User not found.");
    }

    public async Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await deleteValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var user = await context.Users
            .SingleOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new ForbiddenException("Password is incorrect.");
        }

        user.Username = $"deleted-{Base64Url.EncodeToString(user.Id.ToByteArray())}";
        user.Email = $"{user.Id:N}@deleted.invalid";
        user.PasswordHash = string.Empty;

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);
    }
}
