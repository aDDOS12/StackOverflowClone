using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StackOverflowClone.Application.Common.Exceptions;

namespace StackOverflowClone.Api.ErrorHandling;

public sealed class ApplicationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        (int StatusCode, string Title)? mapping = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict."),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized."),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden."),
            _ => null
        };

        if (mapping is null)
        {
            return false;
        }

        var (statusCode, title) = mapping.Value;
        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message
            }
        });
    }
}
