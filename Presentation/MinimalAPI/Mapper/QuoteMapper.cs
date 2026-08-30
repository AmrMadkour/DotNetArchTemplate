using Domain.Models;
using MinimalAPI.Dtos;

namespace MinimalAPI.Mapper;

public static class QuoteMapper
{
    public static QuoteDto ToDto(this Quote quote) => new()
    {
        Subtotal = quote.Subtotal,
        DiscountAmount = quote.DiscountAmount,
        Total = quote.Total,
        AppliedCoupon = quote.AppliedCoupon
    };
}
