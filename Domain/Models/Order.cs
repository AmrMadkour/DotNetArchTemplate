namespace Domain.Models;

public class Order
{
    public List<Item>? Items { get; set; }
    public string? CouponCode { get; set; }

    public string? Validate()
    {
        if (Items == null || Items.Count == 0)
        {
            return "Order must contain at least one item.";
        }

        return Items.Select(item => item.Validate())
            .FirstOrDefault(error => error != null);
    }

    public decimal CalculateSubtotal()
    {
        return Items!.Sum(item => item.CalculateSubtotal());
    }
}
