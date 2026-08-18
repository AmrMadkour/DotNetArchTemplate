using API.Dtos;
using Domain.Models;

namespace API.Mapper;

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
