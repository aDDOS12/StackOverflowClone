using Microsoft.IdentityModel.JsonWebTokens;
using StackOverflowClone.Application.Common.Interfaces;

namespace StackOverflowClone.Api.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var subject = httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return Guid.TryParse(subject, out var userId) ? userId : null;
        }
    }
}
