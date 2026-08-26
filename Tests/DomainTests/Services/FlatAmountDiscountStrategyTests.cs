using Domain.Models;
using Domain.Services;

namespace Tests.DomainTests.Services;

public class FlatAmountDiscountStrategyTests
{
    [Fact]
    public void WhenDiscountType_ShouldBeFlatAmount()
    {
        //Arrange
        var strategy = new FlatAmountDiscountStrategy();

        //Act
        var discountType = strategy.discountType;

        //Assert
        Assert.Equal(DiscountType.FlatAmount, discountType);
    }

    [Fact]
    public void WhenAmountLessThanSubtotal_Calculate_ShouldReturnAmount()
    {
        //Arrange
        var strategy = new FlatAmountDiscountStrategy();

        //Act
        var discount = strategy.Calculate(100, 20);

        //Assert
        Assert.Equal(20, discount);
    }
}
