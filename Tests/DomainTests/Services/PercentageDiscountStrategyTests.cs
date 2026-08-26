using Domain.Services;

namespace Tests.DomainTests.Services;

public class PercentageDiscountStrategyTests
{
    [Fact]
    public void WhenCalculate_ShouldReturnSubtotalTimesAmount()
    {
        //Arrange
        var strategy = new PercentageDiscountStrategy();

        //Act
        var discount = strategy.Calculate(200, 0.10m);

        //Assert
        Assert.Equal(20, discount);
    }
}
