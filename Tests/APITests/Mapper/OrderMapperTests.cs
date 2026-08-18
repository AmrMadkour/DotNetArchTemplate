using API.Dtos;
using API.Mapper;
using Tests.Builders;

namespace Tests.APITests.Mapper;

public class OrderMapperTests
{
    [Fact]
    public void WhenValidOrderDto_ToDomain_ShouldMapAllFields()
    {
        //Arrange
        var dto = new OrderDto
        {
            Items = new List<ItemDto> { new ItemDtoBuilder().Build() },
            CouponCode = "SAVE10"
        };

        //Act
        var order = dto.ToDomain();

        //Assert
        Assert.Single(order.Items!);
        Assert.Equal("P1", order.Items![0].ProductId);
        Assert.Equal(10, order.Items[0].UnitPrice);
        Assert.Equal(2, order.Items[0].Quantity);
        Assert.Equal("SAVE10", order.CouponCode);
    }

    [Fact]
    public void WhenItemsIsNull_ToDomain_ShouldReturnNullItems()
    {
        //Arrange
        var dto = new OrderDto { Items = null, CouponCode = null };

        //Act
        var order = dto.ToDomain();

        //Assert
        Assert.Null(order.Items);
        Assert.Null(order.CouponCode);
    }
}
