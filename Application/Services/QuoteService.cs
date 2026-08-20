using Application.Results;
using Domain.Models;

namespace Application.Services;

public class QuoteService : IQuoteService
{
    public QuoteService()
    {

    }
    public async Task<Result<Quote>> PrepareQuote(Order order)
    {
        var quote = new Quote();

        if (order == null)
        {
            return Result<Quote>.Failure("Order must contain at least one item.");
        }
        var error = order.Validate();
        if (error != null)
        {
            return Result<Quote>.Failure(error);
        }
        quote.Subtotal = order.CalculateSubtotal();
        switch (order.CouponCode)
        {
            case "SAVE10":
                if (quote.Subtotal >= 100)
                {
                    quote.DiscountAmount = quote.Subtotal * 0.10m;
                    quote.AppliedCoupon = order.CouponCode;
                }
                else
                {
                    quote.DiscountAmount = 0;
                }
                break;
            case "SAVE20":
                if (quote.Subtotal >= 200)
                {
                    quote.DiscountAmount = quote.Subtotal * 0.20m;
                    quote.AppliedCoupon = order.CouponCode;
                }
                else
                {
                    quote.DiscountAmount = 0;
                }
                break;
            case null:
                quote.DiscountAmount = 0;
                break;
            case "":
                quote.DiscountAmount = 0;
                break;
            default:
                return Result<Quote>.Failure("Invalid coupon code.");
        }

        quote.Total = quote.Subtotal - quote.DiscountAmount;

        return Result<Quote>.Success(quote);
    }
}
