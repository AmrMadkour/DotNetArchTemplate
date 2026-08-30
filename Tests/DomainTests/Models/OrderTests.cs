using Domain.Models;
using FluentAssertions;
using Tests.Builders;

namespace Tests.DomainTests.Models;

public class OrderTests
{
    [Fact]
    public void Validate_WhenOrderIsValid_ShouldReturnNull()
    {
        //Arrange
        var order = new OrderBuilder().Build();

        //Act

        //Assert
        Assert.Null(order.Validate());

        //order.Validate().Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(InvalidItemsData))]
    public void Validate_WhenItemsIsNullOrEmpty_ShouldReturnError(List<Item>? items)
    {
        //Arrange
        var expectedError = "Order must contain at least one item.";
        var order = new OrderBuilder().WithItems(items).Build();

        //Act

        //Assert
        Assert.Equal(expectedError, order.Validate());

        //order.Validate().Should().BeSameAs(expectedError);
    }

    [Fact]
    public void Validate_WhenItemInvalid_ShouldReturnItemError()
    {
        //Arrange
        var order = new OrderBuilder()
            .WithItems([new ItemBuilder().WithQuantity(0).Build()])
            .Build();

        //Act
        var result = order.Validate();

        //Assert
        Assert.NotNull(result);
        Assert.IsType<string>(result);

        //result.Should().NotBeNull();
        //result.Should().BeOfType<string>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("SAVE10")]
    public void Validate_WhenCouponCodeIsNullOrEmptyOrIsValid_ShouldReturnNull(string? couponCode)
    {
        //Arrange
        var order = new OrderBuilder()
            .WithItems([new ItemBuilder().Build()])
            .WithCouponCode(couponCode)
            .Build();

        //Act
        var result = order.Validate();

        //Assert
        Assert.Null(result);

        //result.Should().BeNull();

    }

    [Fact]
    public void Validate_WhenCouponCodeInvalid_ShouldReturnError()
    {
        //Arrange
        var expectedError = "Invalid coupon code.";
        var order = new OrderBuilder().WithCouponCode("INVALID").Build();

        //Act

        //Assert
        Assert.Equal(expectedError, order.Validate());

        //order.Validate().Should().BeSameAs(expectedError);
    }

    [Fact]
    public void CalculateSubtotal_WhenItemsValid_ShouldSumItemsSubtotals()
    {
        //Arrange
        var expectedSubtotal = 25m;
        var order = new OrderBuilder()
            .WithItems(new List<Item>
            {
                new ItemBuilder().WithQuantity(2).WithUnitPrice(10).Build(),
                new ItemBuilder().WithQuantity(1).WithUnitPrice(5).Build()
            })
            .Build();

        //Act

        //Assert
        Assert.Equal(expectedSubtotal, order.CalculateSubtotal());

        //order.CalculateSubtotal().Should().Be(expectedSubtotal);
    }

    [Fact]
    public void CalculateSubtotal_WhenItemsIsNull_ShouldThrow()
    {
        //Arrange
        var order = new OrderBuilder().WithItems(null).Build();

        //Act

        //Assert
        Assert.Throws<ArgumentNullException>(() => order.CalculateSubtotal());

        //var act = () => order.CalculateSubtotal();
        //act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("InValid")]
    public void CalculateDiscount_WhenCouponCodeIsNullOrEmptyOrIsInValid_ShouldReturnZero(string? couponCode)
    {
        //Arrange
        var subtotal = 500m;
        var expectedDiscount = 0m;
        var order = new OrderBuilder().WithCouponCode(couponCode).Build();

        //Act

        //Assert
        Assert.Equal(expectedDiscount, order.CalculateDiscount(subtotal));

        //order.CalculateDiscount(subtotal).Should().Be(expectedDiscount);
    }

    [Theory]
    [InlineData(500, 50)]
    [InlineData(100, 10)]
    public void CalculateDiscount_WhenCouponCodeIsValidAndSubtotalGreaterThanOrEqualMinSubtotal_ShouldReturnPercentageOff(decimal subtotal, decimal expectedDiscount)
    {
        //Arrange
        var order = new OrderBuilder().Build();

        //Act

        //Assert
        Assert.Equal(expectedDiscount, order.CalculateDiscount(subtotal));

        //order.CalculateDiscount(subtotal).Should().Be(expectedDiscount);
    }

    [Fact]
    public void CalculateDiscount_WhenCouponCodeIsValidAndSubtotalLessThanMinSubtotal_ShouldReturnZero()
    {
        //Arrange
        var subtotal = 10m;
        var expectedDiscount = 0m;
        var order = new OrderBuilder().Build();

        //Act

        //Assert
        Assert.Equal(expectedDiscount, order.CalculateDiscount(subtotal));

        //order.CalculateDiscount(subtotal).Should().Be(expectedDiscount);
    }

    public static TheoryData<List<Item>?> InvalidItemsData =>
    [
            null,
            []
    ];
}
