using Tests.Builders;

namespace Tests.DomainTests;

public class ItemTests
{
    [Fact]
    public void WhenQuantityAndUnitPriceSet_CalculateSubtotal_ShouldReturnQuantityTimesUnitPrice()
    {
        //Arrange
        var item = new ItemBuilder().WithQuantity(3).WithUnitPrice(10).Build();

        //Act
        var subtotal = item.CalculateSubtotal();

        //Assert
        Assert.Equal(30, subtotal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void WhenQuantityIsZeroOrNegative_Validate_ShouldReturnError(int quantity)
    {
        //Arrange
        var item = new ItemBuilder().WithQuantity(quantity).Build();

        //Act
        var error = item.Validate();

        //Assert
        Assert.Equal("Item quantity must be greater than 0.", error);
    }

    [Fact]
    public void WhenItemIsValid_Validate_ShouldReturnNull()
    {
        //Arrange
        var item = new ItemBuilder().Build();

        //Act
        var error = item.Validate();

        //Assert
        Assert.Null(error);
    }
}
