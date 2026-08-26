using Domain.Models;

namespace Tests.Builders;

public class ItemBuilder
{
    private string? _productId = "P1";
    private decimal _unitPrice = 10;
    private int _quantity = 1;

    public ItemBuilder WithProductId(string? productId)
    {
        _productId = productId;
        return this;
    }

    public ItemBuilder WithUnitPrice(decimal unitPrice)
    {
        _unitPrice = unitPrice;
        return this;
    }

    public ItemBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public Item Build() => new() { ProductId = _productId, UnitPrice = _unitPrice, Quantity = _quantity };
}
