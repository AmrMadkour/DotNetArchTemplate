namespace Domain.Models;

// Only used by the parked Strategy/Factory scaffold (see IDiscountStrategy),
// not by the current CouponCatalog-based discount calc — see Order.CalculateDiscount
// for why. Add a case here once a discount type needs its own formula, not just its
// own numbers.
public enum DiscountType
{
    Percentage,
    FlatAmount
}
