using API.Dtos;
using API.Mapper;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class OrdersController(
    IQuoteService quoteService,
    IHttpClientFactory httpClientFactory,
    TimeProvider timeProvider,
    ILogger<OrdersController> logger) : ControllerBase
{
    [Authorize]
    [EnableRateLimiting("PerUser")]
    [HttpPost(Name = "quote")]
    public async Task<ActionResult<QuoteDto>> Quote(OrderDto orderDto)
    {
        var order = orderDto.ToDomain();
        var result = await quoteService.PrepareQuote(order);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        await CallMinimalApiLoadLogsForTest();
        DemoTimeProviderVsDateTimeNow();

        return Ok(result.Value.ToDto());
    }

    // Manual-verification call, treating MinimalAPI as if it were a separate service: forwards this
    // request's TraceId/CorrelationId so its log line can be tied back to this one.
    private async Task CallMinimalApiLoadLogsForTest()
    {
        var client = httpClientFactory.CreateClient("MinimalApi");
        var request = new HttpRequestMessage(HttpMethod.Get, "/diagnostics/load-logs-for-test");
        request.Headers.Add("X-Trace-Id", HttpContext.TraceIdentifier);
        request.Headers.Add("X-Correlation-Id", HttpContext.Items["CorrelationId"]?.ToString());

        await client.SendAsync(request);
    }

    // Manual-verification demo: DateTime.UtcNow reads the real system clock directly, so any logic
    // branching on "now" (expiry checks, scheduling, audit timestamps) can't be controlled from a test —
    // there's no way to make DateTime.UtcNow return a specific instant without sleeping the test thread
    // or wrapping it yourself. TimeProvider is injected instead (registered as TimeProvider.System in
    // Program.cs), so a unit test can substitute Microsoft.Extensions.TimeProvider.Testing's
    // FakeTimeProvider and set "now" to whatever instant the test needs, exercising the same production
    // code path deterministically.
    private void DemoTimeProviderVsDateTimeNow()
    {
        var systemNow = DateTime.UtcNow;
        var providerNow = timeProvider.GetUtcNow();

        logger.LogInformation(
            "TimeProvider demo — DateTime.UtcNow: {SystemNow}, TimeProvider.GetUtcNow(): {ProviderNow}",
            systemNow, providerNow);
    }
}
