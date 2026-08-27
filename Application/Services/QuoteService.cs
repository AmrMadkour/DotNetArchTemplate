using Application.Results;
using Domain.Models;
using Domain.Services;

namespace Application.Services;

// Live path: every coupon today is a percentage, so a plain lookup is enough.
// If a coupon ever needs a different formula (e.g. flat-amount off), this
// constructor-inject-and-call is how the parked Strategy scaffold would plug
public class QuoteService /*(DiscountStrategyFactory discountStrategyFactory)*/ : IQuoteService
{
    // No real I/O yet, so this runs synchronously despite the async signature;
    // add real awaits once Infrastructure I/O (e.g. a repository call) lands here.
    public async Task<Result<Quote>> PrepareQuote(Order order)
    {
        var validationResultMessage = ValidateOrder(order);
        if (validationResultMessage != null)
        {
            return Result<Quote>.Failure(validationResultMessage);
        }

        var quote = new Quote
        {
            Subtotal = order.CalculateSubtotal()
        };

        //   var strategy = _discountStrategyFactory.Resolve(coupon.DiscountType);
        //   quote.DiscountAmount = strategy.Calculate(quote.Subtotal, coupon.Amount);

        quote.DiscountAmount = order.CalculateDiscount(quote.Subtotal);
        quote.AppliedCoupon = quote.DiscountAmount > 0 ? order.CouponCode : null;
        quote.Total = quote.Subtotal - quote.DiscountAmount;

        return Result<Quote>.Success(quote);
    }

    private static string? ValidateOrder(Order order)
    {
        if (order == null)
        {
            return "Order can not be null.";
        }
        var error = order.Validate();
        if (error != null)
        {
            return error;
        }
        return null;
    }

}
