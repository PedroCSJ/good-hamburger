namespace GoodHamburger.Api.Domain;

public class OrderItem
{
    public string MenuItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public MenuItemType Type { get; set; }
}
