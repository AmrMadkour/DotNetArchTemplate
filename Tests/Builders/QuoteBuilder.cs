using Domain.Models;

namespace Tests.Builders;

public class QuoteBuilder
{
    private decimal _subtotal = 10;
    private decimal _discountAmount = 0;
    private decimal _total = 10;
    private string? _appliedCoupon = "SAVE10";

    public QuoteBuilder WithSubtotal(decimal subtotal)
    {
        _subtotal = subtotal;
        return this;
    }

    public QuoteBuilder WithDiscountAmount(decimal discountAmount)
    {
        _discountAmount = discountAmount;
        return this;
    }

    public QuoteBuilder WithTotal(decimal total)
    {
        _total = total;
        return this;
    }

    public QuoteBuilder WithAppliedCoupon(string? appliedCoupon)
    {
        _appliedCoupon = appliedCoupon;
        return this;
    }

    public Quote Build() => new()
    {
        Subtotal = _subtotal,
        DiscountAmount = _discountAmount,
        Total = _total,
        AppliedCoupon = _appliedCoupon
    };
}
