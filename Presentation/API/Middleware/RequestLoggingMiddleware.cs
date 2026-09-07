namespace API.Middleware;

// TraceId = ASP.NET Core's own per-request id (same one GlobalExceptionHandler reports back to the caller).
// CorrelationId = a second, independently generated id. In a real project you'd only keep one of these —
// both exist here side by side purely so the difference between the two is visible in the logs.
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        var correlationId = Guid.NewGuid().ToString();

        // BeginScope attaches these values to every log line written while the scope is open —
        // including ones written from Application/Infrastructure via their own injected ILogger<T> —
        // without passing traceId/correlationId as parameters through every method call in between.
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = traceId,
            ["CorrelationId"] = correlationId
        }))
        {
            logger.LogInformation("Request started {Method} {Path}", context.Request.Method, context.Request.Path);

            await next(context);

            logger.LogInformation("Request finished {Method} {Path} with status {StatusCode}", context.Request.Method, context.Request.Path, context.Response.StatusCode);
        }
    }
}
