namespace Domain.Models;

public class Item
{
    public string? ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public string? Validate()
    {
        return ValidateQuantity() ?? ValidateUnitPrice();
    }

    public decimal CalculateSubtotal()
    {
        return Quantity * UnitPrice;
    }

    private string? ValidateQuantity()
    {
        return Quantity <= 0 ? "Item quantity must be greater than 0." : null;
    }

    private string? ValidateUnitPrice()
    {
        return UnitPrice < 0 ? "Item unit price cannot be negative." : null;
    }
}
