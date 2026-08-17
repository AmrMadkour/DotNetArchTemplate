namespace API.Dtos;

public class OrderDto
{
    public List<ItemDto>? Items { get; set; }
    public string? CouponCode { get; set; }
}
