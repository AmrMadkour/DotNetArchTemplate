namespace MinimalAPI.Endpoints;

// Manual-verification tool for cross-service tracing, not a real business endpoint. Called by API's
// OrdersController after preparing a quote, forwarding that request's TraceId/CorrelationId as headers.
// RequestLoggingMiddleware reads those headers and continues the trace, so this log line — and every
// other one this request produces — carries the same TraceId/CorrelationId as the request in API.
public static class LoadLogsForTestEndpoint
{
    public static void MapLoadLogsForTestEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/diagnostics/load-logs-for-test", (ILogger<Program> logger) =>
        {
            logger.LogInformation("LoadLogsForTest called by another service");
            return Results.Ok();
        });
    }
}
