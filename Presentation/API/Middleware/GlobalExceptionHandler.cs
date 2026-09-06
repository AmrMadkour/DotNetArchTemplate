using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

// Catch-all for unexpected/programmer-error exceptions per Rule 8 — expected failures never reach here, they return Result<T>.
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    // Registered via AddExceptionHandler<T> + UseExceptionHandler in Program.cs — ASP.NET Core invokes this
    // instead of the developer exception page/default 500 once an unhandled exception escapes MVC's pipeline.
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Full exception (with stack trace) goes to the log; only the generic message below reaches the client.
        logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        // Deliberately generic — the real exception detail is not leaked to the caller, only to the log above.
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        // IProblemDetailsService (registered via AddProblemDetails in Program.cs) applies the
        // CustomizeProblemDetails callback there (adds traceId) before writing the RFC 7807 body.
        // Returning true tells the middleware the exception was handled, so it won't rethrow.
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }
}
