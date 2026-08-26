using Domain.Models;

namespace Domain.Services;

// Parked half of the deferred Strategy/Factory design (see DiscountType) — one
// implementation per DiscountType, each owning its own formula. Not referenced by
// QuoteService/Order today; the live path is Order.CalculateDiscount using
// CouponCatalog, which is enough while every coupon is a flat percentage. Wire this
// in once a coupon needs a genuinely different formula (e.g. flat-amount off), not
// just different numbers.
public interface IDiscountStrategy
{
    DiscountType discountType { get; }

    decimal Calculate(decimal subtotal, decimal amount);
}
