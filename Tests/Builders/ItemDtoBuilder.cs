using API.Dtos;

namespace Tests.Builders;

public class ItemDtoBuilder
{
    private string? _productId = "P1";
    private decimal _unitPrice = 10;
    private int _quantity = 2;

    public ItemDtoBuilder WithProductId(string? productId)
    {
        _productId = productId;
        return this;
    }

    public ItemDtoBuilder WithUnitPrice(decimal unitPrice)
    {
        _unitPrice = unitPrice;
        return this;
    }

    public ItemDtoBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public ItemDto Build() => new() { ProductId = _productId, UnitPrice = _unitPrice, Quantity = _quantity };
}
