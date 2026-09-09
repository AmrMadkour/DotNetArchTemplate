namespace MinimalAPI.Middleware;

// TraceId = ASP.NET Core's own per-request id (same one GlobalExceptionHandler reports back to the caller).
// CorrelationId = a second, independently generated id. In a real project you'd only keep one of these —
// both exist here side by side purely so the difference between the two is visible in the logs.
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Continue an inbound trace when another service forwarded one (e.g. via API's OrdersController
        // calling this project) instead of always minting a fresh pair, so every log line this request
        // produces — not just one — ties back to the request that triggered it.
        var traceId = context.Request.Headers.TryGetValue("X-Trace-Id", out var forwardedTraceId) && !string.IsNullOrEmpty(forwardedTraceId)
            ? forwardedTraceId.ToString()
            : context.TraceIdentifier;
        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var forwardedCorrelationId) && !string.IsNullOrEmpty(forwardedCorrelationId)
            ? forwardedCorrelationId.ToString()
            : Guid.NewGuid().ToString();

        // BeginScope attaches these values to every log line written while the scope is open —
        // including ones written from Application/Infrastructure via their own injected ILogger<T> —
        // without passing traceId/correlationId as parameters through every method call in between.
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = traceId,
            ["CorrelationId"] = correlationId
        }))
        {
            logger.LogInformation("MinimalAPI request started {Method} {Path}", context.Request.Method, context.Request.Path);

            await next(context);

            logger.LogInformation("MinimalAPI request finished {Method} {Path} with status {StatusCode}", context.Request.Method, context.Request.Path, context.Response.StatusCode);
        }
    }
}
