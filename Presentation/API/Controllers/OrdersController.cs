using API.Dtos;
using API.Mapper;
using Application;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class OrdersController(
    IQuoteService quoteService) : ControllerBase
{
    private readonly IQuoteService _quoteService = quoteService;

    [HttpPost(Name = "quote")]
    public ActionResult<QuoteDto> Quote(OrderDto orderdto)
    {
        var order = orderdto.ToDomain();
        _quoteService.PrepareQuote(orderdto);
        return Ok(new QuoteDto());
    }
}
