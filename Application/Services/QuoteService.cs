using Application.Results;
using Domain.Models;

namespace Application.Services;

public class QuoteService : IQuoteService
{
    public async Task<Result<Quote>> PrepareQuote(Order order)
    {
        if (order == null)
        {
            return Result<Quote>.Failure("Order can not be null.");
        }

        var error = order.Validate();
        if (error != null)
        {
            return Result<Quote>.Failure(error);
        }

        var quote = new Quote
        {
            Subtotal = order.CalculateSubtotal()
        };
        // Live path: every coupon today is a percentage, so a plain lookup is enough.
        // If a coupon ever needs a different formula (e.g. flat-amount off), this
        // constructor-inject-and-call is how the parked Strategy scaffold would plug
        // in instead:
        //
        //   public QuoteService(DiscountStrategyFactory discountStrategyFactory)
        //   {
        //       _discountStrategyFactory = discountStrategyFactory;
        //   }
        //   ...
        //   var strategy = _discountStrategyFactory.Resolve(coupon.DiscountType);
        //   quote.DiscountAmount = strategy.Calculate(quote.Subtotal, coupon.Amount);
        quote.DiscountAmount = order.CalculateDiscount(quote.Subtotal);
        quote.AppliedCoupon = quote.DiscountAmount > 0 ? order.CouponCode : null;
        quote.Total = quote.Subtotal - quote.DiscountAmount;

        return Result<Quote>.Success(quote);
    }
}
