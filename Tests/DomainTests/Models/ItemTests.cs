using FluentAssertions;
using Tests.Builders;

namespace Tests.DomainTests.Models;

public class ItemTests
{
    [Fact]
    public void Validate_WhenItemIsValid_ShouldReturnNull()
    {
        //Arrange
        var item = new ItemBuilder().Build();

        //Act

        //Assert
        Assert.Null(item.Validate());

        //item.Validate().Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenQuantityIsZeroOrNegative_ShouldReturnError(int quantity)
    {
        //Arrange
        var expectedError = "Item quantity must be greater than 0.";
        var item = new ItemBuilder().WithQuantity(quantity).Build();

        //Act

        //Assert
        Assert.Equal(expectedError, item.Validate());

        //item.Validate().Should().Be(expectedError);
    }

    [Fact]
    public void Validate_WhenUnitPriceIsNegative_ShouldReturnError()
    {
        //Arrange
        var expectedError = "Item unit price cannot be negative.";
        var item = new ItemBuilder().WithUnitPrice(-1m).Build();

        //Act

        //Assert
        Assert.Equal(expectedError, item.Validate());

        //item.Validate().Should().Be(expectedError);
    }

    [Fact]
    public void CalculateSubtotal_WhenQuantityAndUnitPriceSet_ShouldReturnQuantityTimesUnitPrice()
    {
        //Arrange
        var expectedSubtotal = 30m;
        var item = new ItemBuilder().WithQuantity(3).WithUnitPrice(10).Build();

        //Act

        //Assert
        Assert.Equal(expectedSubtotal, item.CalculateSubtotal());

        //item.CalculateSubtotal().Should().Be(expectedSubtotal);
    }
}
