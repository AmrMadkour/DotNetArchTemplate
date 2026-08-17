namespace Domain.Models;

public class Order
{
    public List<Item>? Items { get; set; }
    public string? CouponCode { get; set; }
}
