using API.Mapper;
using FluentAssertions;
using Tests.Builders;

namespace Tests.APITests.Mapper;

public class ItemMapperTests
{
    [Fact]
    public void ToDomain_WhenValidItemDto_ShouldMapAllFields()
    {
        //Arrange
        var expectedItem = new ItemBuilder().Build();

        //Act
        var item = new ItemDtoBuilder().Build().ToDomain();

        //Assert
        Assert.Equivalent(expectedItem, item);

        //item.Should().BeEquivalentTo(sexpectedItem);
    }
}
