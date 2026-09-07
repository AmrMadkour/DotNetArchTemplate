using Application.Services;

namespace MinimalAPI.Endpoints;

// Manual-verification tool for the logging pipeline, not a real business endpoint — see LogTestService.
public static class DiagnosticsEndpoints
{
    public static void MapDiagnosticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/diagnostics");

        group.MapGet("/log-test", async (ILogTestService logTestService, bool fail = false) =>
        {
            await logTestService.RunAsync(fail);
            return Results.Ok();
        });
    }
}
