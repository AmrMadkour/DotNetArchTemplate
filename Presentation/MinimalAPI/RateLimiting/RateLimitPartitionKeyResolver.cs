using System.Security.Claims;

namespace MinimalAPI.RateLimiting;

// Pulled out of the AddRateLimiter lambda so it's unit-testable on its own.
public static class RateLimitPartitionKeyResolver
{
    public static string ResolveUserOrIpKey(HttpContext context)
    {
        return context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous"
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    public static string ResolveIpKey(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
