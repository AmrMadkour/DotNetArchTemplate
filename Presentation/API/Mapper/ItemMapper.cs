using API.Dtos;
using Domain.Models;

namespace API.Mapper;

public static class ItemMapper
{
    public static Item ToDomain(this ItemDto dto) => new()
    {
        ProductId = dto.ProductId,
        UnitPrice = dto.UnitPrice,
        Quantity = dto.Quantity
    };
}
