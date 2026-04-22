namespace GoodHamburger.Api.Domain;

public class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public List<OrderItem> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public int DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
