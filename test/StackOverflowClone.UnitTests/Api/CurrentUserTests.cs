using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using NSubstitute;
using StackOverflowClone.Api.Services;

namespace StackOverflowClone.UnitTests.Api;

public sealed class CurrentUserTests
{
    [Fact]
    public void UserId_WithValidSubClaim_ReturnsUserID()
    {
        var userId = Guid.NewGuid();
        var accessor = CreateAccessor(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()));

        var currentUser = new CurrentUser(accessor);

        Assert.Equal(userId, currentUser.UserId);
    }

    [Fact]
    public void UserId_WIthoutSubClaim_ReturnsNull()
    {
        var currentUser = new CurrentUser(CreateAccessor());

        Assert.Null(currentUser.UserId);
    }

    [Fact]
    public void UserId_WithInvalidSubClaim_ReturnsNull()
    {
        var accessor = CreateAccessor(new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid"));

        var currentUser = new CurrentUser(accessor);

        Assert.Null(currentUser.UserId);
    }

    [Fact]
    public void UserId_WithoutHttpContext_ReturnsNull()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var currentUser = new CurrentUser(accessor);

        Assert.Null(currentUser.UserId);
    }

    private static IHttpContextAccessor CreateAccessor(params Claim[] claims)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        return accessor;
    }
}
