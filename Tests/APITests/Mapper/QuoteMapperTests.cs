using API.Mapper;
using Domain.Models;
using FluentAssertions;
using Tests.Builders;

namespace Tests.APITests.Mapper;

public class QuoteMapperTests
{
    [Fact]
    public void ToDto_WhenValidQuote_ShouldMapAllFields()
    {
        //Arrange
        var expectedQuoteDto = new QuoteDtoBuilder().Build();

        //Act
        var quoteDto = new QuoteBuilder().Build().ToDto();

        //Assert
        Assert.Equivalent(expectedQuoteDto, quoteDto);

        //quoteDto.Should().BeEquivalentTo(expectedQuoteDto);
    }
}
