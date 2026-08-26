namespace Domain.Models;

public class Order
{
    public List<Item>? Items { get; set; }
    public string? CouponCode { get; set; }

    public decimal CalculateSubtotal()
    {
        return Items!.Sum(item => item.CalculateSubtotal());
    }
    public decimal CalculateDiscount(decimal subtotal)
    {
        if (string.IsNullOrEmpty(CouponCode) || !TryResolveCoupon(out var coupon))
        {
            return 0;
        }

        return subtotal >= coupon!.MinSubtotal ? subtotal * coupon.Percentage : 0;
    }
    public string? Validate()
    {
        return ValidateItems() ?? ValidateCouponCode();
    }


    private string? ValidateItems()
    {
        if (Items == null || Items.Count == 0)
        {
            return "Order must contain at least one item.";
        }
        return Items.Select(item => item.Validate())
            .FirstOrDefault(error => error != null);
    }
    private string? ValidateCouponCode()
    {
        if (string.IsNullOrEmpty(CouponCode))
        {
            return null;
        }

        return TryResolveCoupon(out _) ? null : "Invalid coupon code.";
    }
    private bool TryResolveCoupon(out Coupon? coupon)
    {
        return CouponCatalog.Coupons.TryGetValue(CouponCode!, out coupon);
    }
}
