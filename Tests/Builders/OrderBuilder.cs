using Domain.Models;

namespace Tests.Builders;

public class OrderBuilder
{
    private List<Item>? _items = [new ItemBuilder().Build()];
    private string? _couponCode = "SAVE10";

    public OrderBuilder WithItems(List<Item>? items)
    {
        _items = items;
        return this;
    }

    public OrderBuilder WithCouponCode(string? couponCode)
    {
        _couponCode = couponCode;
        return this;
    }

    public Order Build() => new() { Items = _items, CouponCode = _couponCode };
}
