using API.Dtos;
using Domain.Models;

namespace API.Mapper;

public static class OrderMapper
{
    public static Order ToDomain(this OrderDto dto) => new()
    {
        Items = dto.Items?.Select(item => item.ToDomain()).ToList(),
        CouponCode = dto.CouponCode
    };
}
