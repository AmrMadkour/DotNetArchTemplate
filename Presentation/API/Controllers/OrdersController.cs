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
    IHttpClientFactory httpClientFactory) : ControllerBase
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
}
