using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Application.Common.Exceptions;
using StackOverflowClone.Application.Common.Interfaces;

namespace StackOverflowClone.Application.Common;

internal static class CurrentUserExtensions
{
    public static async Task<Guid> GetActiveUserIdAsync(this ICurrentUser currentUser, IApplicationDbContext context, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var userExists = await context.Users.AnyAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);

        return userExists ? userId : throw new UnauthorizedException("User account no longer exists.");
    }
}
