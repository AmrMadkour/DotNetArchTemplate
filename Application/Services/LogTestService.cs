using Microsoft.Extensions.Logging;

namespace Application.Services;

// Manual-verification tool for the logging pipeline (Presentation -> Application) — not unit tested,
// same way you'd click through a UI rather than write a test for it. Proves the trace/correlation ID
// set by the presentation-layer middleware shows up in a log line written from inside this layer too.
public class LogTestService(ILogger<LogTestService> logger) : ILogTestService
{
    public Task RunAsync(bool fail)
    {
        logger.LogInformation("LogTestService started");

        if (fail)
        {
            throw new InvalidOperationException("Intentional failure to exercise the Error-level logging path.");
        }

        logger.LogInformation("LogTestService finished");
        return Task.CompletedTask;
    }
}
