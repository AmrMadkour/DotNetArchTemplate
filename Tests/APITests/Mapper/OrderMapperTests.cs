using API.Dtos;
using API.Mapper;
//using FluentAssertions;
using Tests.Builders;

namespace Tests.APITests.Mapper;

public class OrderMapperTests
{
    [Fact]
    public void ToDomain_WhenValidOrderDto_ShouldMapAllFields()
    {
        //Arrange
        var expectedOrder = new OrderBuilder().Build();

        //Act
        var order = new OrderDtoBuilder().Build().ToDomain();

        //Assert
        Assert.Equivalent(expectedOrder, order);

        //order.Should().BeEquivalentTo(expectedOrder);
    }

    [Fact]
    public void ToDomain_WhenItemsIsNull_ShouldReturnNullItems()
    {
        //Arrange

        //Act
        var order = new OrderDtoBuilder().WithItems(null).Build().ToDomain();

        //Assert
        Assert.Null(order.Items);

        //order.Items.Should().BeNull();
    }
}
