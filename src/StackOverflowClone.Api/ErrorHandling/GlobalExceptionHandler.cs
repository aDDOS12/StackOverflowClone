using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace StackOverflowClone.Api.ErrorHandling
{
    public class GlobalExceptionHandler(IProblemDetailsService problemDetailService, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "An unhandled exception occurred.");

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            return await problemDetailService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred."
                }
            });
        }
    }
}
