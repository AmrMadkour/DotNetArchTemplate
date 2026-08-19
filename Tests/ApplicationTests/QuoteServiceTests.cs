using Application.Exceptions;
using Application.Services;
using Domain.Models;
using Tests.Builders;

namespace Tests.ApplicationTests;

public class QuoteServiceTests
{
    [Theory]
    [MemberData(nameof(InvalidItemsData))]
    public async Task WhenInvaledItemsSent_PrepareQuote_ShouldReturn400(List<Item>? items)
    {
        //Arrange
        var order = new OrderBuilder().WithItems(items).Build();

        var quoteService = new QuoteService();

        //Act

        //Assert
        var ex = await Assert.ThrowsAsync<OrderValidationException>(() => quoteService.PrepareQuote(order));
        Assert.Equal("Order must contain at least one item.", ex.Message);
    }

    public static TheoryData<List<Item>?> InvalidItemsData =>
        new()
        {
            null,
            new List<Item>()
        };
}
