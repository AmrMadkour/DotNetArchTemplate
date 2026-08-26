using Domain.Models;

namespace Domain.Services;

// Part of the parked Strategy scaffold — see IDiscountStrategy for why it's unused.
public class PercentageDiscountStrategy : IDiscountStrategy
{
    public DiscountType discountType => DiscountType.Percentage;

    public decimal Calculate(decimal subtotal, decimal amount)
    {
        return subtotal * amount;
    }
}
