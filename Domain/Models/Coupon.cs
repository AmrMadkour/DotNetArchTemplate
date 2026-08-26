namespace Domain.Models;

public record Coupon(string Code, decimal MinSubtotal, decimal Percentage);

public static class CouponCatalog
{
    public static readonly IReadOnlyDictionary<string, Coupon> Coupons = new Dictionary<string, Coupon>
    {
        ["SAVE10"] = new Coupon("SAVE10", MinSubtotal: 100, Percentage: 0.10m),
        ["SAVE20"] = new Coupon("SAVE20", MinSubtotal: 200, Percentage: 0.20m)
    };
}
