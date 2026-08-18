using API.Dtos;
using API.Mapper;

namespace Tests.APITests.Mapper;

public class ItemMapperTests
{
    [Fact]
    public void WhenValidItemDto_ToDomain_ShouldMapAllFields()
    {
        //Arrange
        var dto = new ItemDto { ProductId = "P1", UnitPrice = 10, Quantity = 2 };

        //Act
        var item = dto.ToDomain();

        //Assert
        Assert.Equal("P1", item.ProductId);
        Assert.Equal(10, item.UnitPrice);
        Assert.Equal(2, item.Quantity);
    }
}
