using Domain.Models;
using Domain.Services;

namespace Tests.DomainTests.Services;

public class DiscountStrategyFactoryTests
{
    [Fact]
    public void WhenStrategyRegistered_Resolve_ShouldReturnMatchingStrategy()
    {
        //Arrange
        var factory = new DiscountStrategyFactory(new IDiscountStrategy[]
        {
            new PercentageDiscountStrategy(),
            new FlatAmountDiscountStrategy()
        });

        //Act
        var strategy = factory.Resolve(DiscountType.Percentage);

        //Assert
        Assert.IsType<PercentageDiscountStrategy>(strategy);
    }
}
