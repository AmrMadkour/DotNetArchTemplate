using API.Dtos;
using Domain.Models;

namespace Tests.Builders;

public class OrderDtoBuilder
{
    private List<ItemDto>? _items = [new ItemDtoBuilder().Build()];
    private string? _couponCode = "SAVE10";

    public OrderDtoBuilder WithItems(List<ItemDto>? items)
    {
        _items = items;
        return this;
    }

    public OrderDtoBuilder WithCouponCode(string? couponCode)
    {
        _couponCode = couponCode;
        return this;
    }

    public OrderDto Build() => new() { Items = _items, CouponCode = _couponCode };
}
