using Domain.Models;

namespace Domain.Services;

// Part of the parked Strategy scaffold — see IDiscountStrategy for why it's unused.
public class FlatAmountDiscountStrategy : IDiscountStrategy
{
    public DiscountType discountType => DiscountType.FlatAmount;

    public decimal Calculate(decimal subtotal, decimal amount)
    {
        // Capped at subtotal so a flat discount can't push Total negative.
        return Math.Min(amount, subtotal);
    }
}
