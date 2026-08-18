using API.Mapper;
using Domain.Models;

namespace Tests.APITests.Mapper;

public class QuoteMapperTests
{
    [Fact]
    public void WhenValidQuote_ToDto_ShouldMapAllFields()
    {
        //Arrange
        var quote = new Quote
        {
            Subtotal = 100,
            DiscountAmount = 10,
            Total = 90,
            AppliedCoupon = "SAVE10"
        };

        //Act
        var dto = quote.ToDto();

        //Assert
        Assert.Equal(100, dto.Subtotal);
        Assert.Equal(10, dto.DiscountAmount);
        Assert.Equal(90, dto.Total);
        Assert.Equal("SAVE10", dto.AppliedCoupon);
    }
}
