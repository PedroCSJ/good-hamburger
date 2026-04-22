namespace GoodHamburger.Api.DTOs;

public class OrderResponse
{
    public Guid Id { get; set; }
    public List<OrderItemResponse> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public int DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class OrderItemResponse
{
    public string MenuItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}
