using Application.Services;
using MinimalAPI.Dtos;
using MinimalAPI.Mapper;

namespace MinimalAPI.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders");

        group.MapPost("/quote", async Task<IResult> (OrderDto orderDto, IQuoteService quoteService) =>
        {
            var order = orderDto.ToDomain();
            var result = await quoteService.PrepareQuote(order);
            return result.IsSuccess
                ? Results.Ok(result.Value.ToDto())
                : Results.BadRequest(result.ErrorMessage);
        });
    }
}
