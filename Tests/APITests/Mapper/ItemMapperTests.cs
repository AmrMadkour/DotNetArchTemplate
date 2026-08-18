using API.Mapper;
using Tests.Builders;

namespace Tests.APITests.Mapper;

public class ItemMapperTests
{
    [Fact]
    public void WhenValidItemDto_ToDomain_ShouldMapAllFields()
    {
        //Arrange
        var dto = new ItemDtoBuilder().Build();

        //Act
        var item = dto.ToDomain();

        //Assert
        Assert.Equal("P1", item.ProductId);
        Assert.Equal(10, item.UnitPrice);
        Assert.Equal(2, item.Quantity);
    }
}
