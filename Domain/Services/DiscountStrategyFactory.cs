using Domain.Models;

namespace Domain.Services
{
    public class DiscountStrategyFactory
    {
     private readonly Dictionary<DiscountType, IDiscountStrategy> _strategies;
        public DiscountStrategyFactory(IEnumerable<IDiscountStrategy> discountStrategies)
        {
            _strategies = discountStrategies.ToDictionary(s => s.discountType);
        }

        public IDiscountStrategy Resolve(DiscountType discountType)
        {
            if (!_strategies.TryGetValue(discountType, out var strategy))
            {
                throw new InvalidOperationException($"No IDiscountStrategy registered for {discountType}.");
            }

            return strategy;
        }

    }
}
