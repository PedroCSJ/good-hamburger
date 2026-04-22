namespace GoodHamburger.Api.Domain;

public class MenuItem
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public MenuItemType Type { get; init; }
}
