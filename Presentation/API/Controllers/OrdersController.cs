using API.Dtos;
using API.Mapper;
using Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class OrdersController(
    IQuoteService quoteService) : ControllerBase
{
    [HttpPost(Name = "quote")]
    public async Task<ActionResult<QuoteDto>> Quote(OrderDto orderDto)
    {
        var order = orderDto.ToDomain();
        var result = await quoteService.PrepareQuote(order);
        return result.IsSuccess
            ? Ok(result.Value.ToDto())
            : BadRequest(result.ErrorMessage);
    }
}
