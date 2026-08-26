using Domain.Models;
using Tests.Builders;

namespace Tests.DomainTests;

public class OrderTests
{
    [Fact]
    public void WhenItemsPresent_CalculateSubtotal_ShouldSumItemSubtotals()
    {
        //Arrange
        var order = new OrderBuilder()
            .WithItems(new List<Item>
            {
                new ItemBuilder().WithQuantity(2).WithUnitPrice(10).Build(),
                new ItemBuilder().WithQuantity(1).WithUnitPrice(5).Build()
            })
            .Build();

        //Act
        var subtotal = order.CalculateSubtotal();

        //Assert
        Assert.Equal(25, subtotal);
    }

    [Fact]
    public void WhenNoCouponCode_CalculateDiscount_ShouldReturnZero()
    {
        //Arrange
        var order = new OrderBuilder().WithCouponCode(null).Build();

        //Act
        var discount = order.CalculateDiscount(500);

        //Assert
        Assert.Equal(0, discount);
    }

    [Fact]
    public void WhenCouponCodeInvalid_CalculateDiscount_ShouldReturnZero()
    {
        //Arrange
        var order = new OrderBuilder().WithCouponCode("INVALID").Build();

        //Act
        var discount = order.CalculateDiscount(500);

        //Assert
        Assert.Equal(0, discount);
    }

    [Fact]
    public void WhenSubtotalMeetsCouponThreshold_CalculateDiscount_ShouldReturnPercentageOff()
    {
        //Arrange
        var order = new OrderBuilder().WithCouponCode("SAVE10").Build();

        //Act
        var discount = order.CalculateDiscount(100);

        //Assert
        Assert.Equal(10, discount);
    }

    [Theory]
    [MemberData(nameof(InvalidItemsData))]
    public void WhenItemsIsNullOrEmpty_Validate_ShouldReturnError(List<Item>? items)
    {
        //Arrange
        var order = new OrderBuilder().WithItems(items).Build();

        //Act
        var error = order.Validate();

        //Assert
        Assert.Equal("Order must contain at least one item.", error);
    }

    public static TheoryData<List<Item>?> InvalidItemsData =>
        new()
        {
            null,
            new List<Item>()
        };

    [Fact]
    public void WhenItemInvalid_Validate_ShouldReturnItemError()
    {
        //Arrange
        var order = new OrderBuilder()
            .WithItems(new List<Item> { new ItemBuilder().WithQuantity(0).Build() })
            .Build();

        //Act
        var error = order.Validate();

        //Assert
        Assert.Equal("Item quantity must be greater than 0.", error);
    }

    [Fact]
    public void WhenCouponCodeInvalid_Validate_ShouldReturnError()
    {
        //Arrange
        var order = new OrderBuilder().WithCouponCode("INVALID").Build();

        //Act
        var error = order.Validate();

        //Assert
        Assert.Equal("Invalid coupon code.", error);
    }
}
