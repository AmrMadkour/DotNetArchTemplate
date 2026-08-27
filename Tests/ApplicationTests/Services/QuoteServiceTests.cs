using Application.Services;
using Domain.Models;
using FluentAssertions;
using Tests.Builders;

namespace Tests.ApplicationTests.Services;

public class QuoteServiceTests
{
    [Fact]
    public async Task PrepareQuote_WhenOrderIsNull_ShouldReturnFailure()
    {
        //Arrange
        var expectedErrorMessage = "Order can not be null.";
        var quoteService = new QuoteService();

        //Act
        var result = await quoteService.PrepareQuote(null!);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorMessage, result.ErrorMessage);

        //result.IsSuccess.Should().BeFalse();
        //result.ErrorMessage.Should().Equals(expectedErrorMessage);
    }

    [Fact]
    public async Task PrepareQuote_WhenOrderNotValid_ShouldReturnFailure()
    {
        //Arrange
        var order = new OrderBuilder().WithItems(null).Build();
        var quoteService = new QuoteService();

        //Act
        var result = await quoteService.PrepareQuote(order);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.IsType<string>(result.ErrorMessage);
        Assert.NotNull(result.ErrorMessage);

        //result.IsSuccess.Should().BeFalse();
        //result.ErrorMessage.Should().BeOfType<string>();
        //result.ErrorMessage.Should().NotBeNull();

    }

    [Fact]
    public async Task PrepareQuote_WhenValidOrderWithoutCouponCode_ShouldReturnSuccessWithoutDiscount()
    {
        //Arrange
        var expectedQuote = new QuoteBuilder().WithAppliedCoupon(null).Build();

        var order = new OrderBuilder().WithCouponCode(null).Build();
        var quoteService = new QuoteService();

        //Act
        var result = await quoteService.PrepareQuote(order);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<Quote>(result.Value);
        Assert.Equivalent(expectedQuote, result.Value);

        //result.IsSuccess.Should().BeTrue();
        //result.Value.Should().BeOfType<Quote>();
        //result.Value.Should().BeEquivalentTo(expectedQuote);
    }

    [Fact]
    public async Task PrepareQuote_WhenValidOrderWithCouponCode_ShouldReturnSuccessWithDiscount()
    {
        //Arrange
        var expectedQuote = new QuoteBuilder().WithSubtotal(100).WithDiscountAmount(10).WithTotal(90).Build();

        var order = new OrderBuilder().WithItems([new ItemBuilder().WithUnitPrice(100).WithQuantity(1).Build()]).Build();
        var quoteService = new QuoteService();

        //Act
        var result = await quoteService.PrepareQuote(order);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<Quote>(result.Value);
        Assert.Equivalent(expectedQuote, result.Value);

        //result.IsSuccess.Should().BeTrue();
        //result.Value.Should().BeOfType<Quote>();
        //result.Value.Should().BeEquivalentTo(expectedQuote);
    }

}
