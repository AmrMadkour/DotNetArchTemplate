using API.Dtos;

namespace Tests.Builders;

public class QuoteDtoBuilder
{
    private decimal _subtotal = 10;
    private decimal _discountAmount = 0;
    private decimal _total = 10;
    private string? _appliedCoupon = "SAVE10";

    public QuoteDtoBuilder WithSubtotal(decimal subtotal)
    {
        _subtotal = subtotal;
        return this;
    }

    public QuoteDtoBuilder WithDiscountAmount(decimal discountAmount)
    {
        _discountAmount = discountAmount;
        return this;
    }

    public QuoteDtoBuilder WithTotal(decimal total)
    {
        _total = total;
        return this;
    }

    public QuoteDtoBuilder WithAppliedCoupon(string? appliedCoupon)
    {
        _appliedCoupon = appliedCoupon;
        return this;
    }

    public QuoteDto Build() => new()
    {
        Subtotal = _subtotal,
        DiscountAmount = _discountAmount,
        Total = _total,
        AppliedCoupon = _appliedCoupon
    };
}
