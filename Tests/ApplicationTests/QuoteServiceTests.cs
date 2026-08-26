using Application.Services;
using Domain.Models;
using Tests.Builders;

namespace Tests.ApplicationTests;

public class QuoteServiceTests
{
    [Fact]
    public async Task WhenOrderIsNull_PrepareQuote_ShouldReturnFailure()
    {
        //Arrange
        var quoteService = new QuoteService();

        //Act
        var result = await quoteService.PrepareQuote(null!);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Order can not be null.", result.ErrorMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidItemsData))]
    public async Task WhenItemsIsNullOrEmpty_PrepareQuote_ShouldReturnFailure(List<Item>? items)
    {
        //Arrange
        var order = new OrderBuilder().WithItems(items).Build();
        var quoteService = new QuoteService();

        //Act
        var result = await quoteService.PrepareQuote(order);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Order must contain at least one item.", result.ErrorMessage);
    }

    public static TheoryData<List<Item>?> InvalidItemsData =>
        new()
        {
            null,
            new List<Item>()
        };

    [Fact]
    public async Task WhenItemInvalid_PrepareQuote_ShouldReturnFailure()
    {
        //Arrange
        var order = new OrderBuilder()
            .WithItems(new List<Item> { new ItemBuilder().WithQuantity(0).Build() })
            .Build();
        var quoteService = new QuoteService();

        //Act
        var result = await quoteService.PrepareQuote(order);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Item quantity must be greater than 0.", result.ErrorMessage);
    }

    [Fact]
    public async Task WhenNoCouponCode_PrepareQuote_ShouldReturnQuoteWithoutDiscount()
    {
        //Arrange
        var order = new OrderBuilder()
            .WithItems(new List<Item> { new ItemBuilder().WithQuantity(1).WithUnitPrice(50).Build() })
            .WithCouponCode(null)
            .Build();
        var quoteService = new QuoteService();

        //Act
        var result = await quoteService.PrepareQuote(order);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(50, result.Value.Subtotal);
        Assert.Equal(0, result.Value.DiscountAmount);
        Assert.Equal(50, result.Value.Total);
        Assert.Null(result.Value.AppliedCoupon);
    }

    [Fact]
    public async Task WhenCouponBelowThreshold_PrepareQuote_ShouldReturnQuoteWithoutDiscount()
    {
        //Arrange
        var order = new OrderBuilder()
            .WithItems(new List<Item> { new ItemBuilder().WithQuantity(1).WithUnitPrice(50).Build() })
            .WithCouponCode("SAVE10")
            .Build();
        var quoteService = new QuoteService();

        //Act
        var result = await quoteService.PrepareQuote(order);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.DiscountAmount);
        Assert.Null(result.Value.AppliedCoupon);
    }
}
