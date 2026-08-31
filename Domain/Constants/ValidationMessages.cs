namespace Domain.Constants;

public static class ValidationMessages
{
    public const string OrderMustHaveAtLeastOneItem = "Order must contain at least one item.";
    public const string InvalidCouponCode = "Invalid coupon code.";
    public const string ItemQuantityMustBePositive = "Item quantity must be greater than 0.";
    public const string ItemUnitPriceCannotBeNegative = "Item unit price cannot be negative.";
    public const string NoDiscountStrategyRegisteredFor = "No IDiscountStrategy registered for {0}.";
}
