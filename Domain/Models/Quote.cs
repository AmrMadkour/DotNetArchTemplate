namespace Domain.Models;

public class Quote
{
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string? AppliedCoupon { get; set; }
}
