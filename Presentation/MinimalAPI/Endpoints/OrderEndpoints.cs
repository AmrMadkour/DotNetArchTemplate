using Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using MinimalAPI.Dtos;
using MinimalAPI.Mapper;

namespace MinimalAPI.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders");

        group.MapPost("/quote", async Task<Results<Ok<QuoteDto>, BadRequest<string>>> (OrderDto orderDto, IQuoteService quoteService) =>
        {
            var order = orderDto.ToDomain();
            var result = await quoteService.PrepareQuote(order);
            return result.IsSuccess
                ? TypedResults.Ok(result.Value.ToDto())
                : TypedResults.BadRequest(result.ErrorMessage);
        });
    }
}
