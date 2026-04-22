namespace GoodHamburger.Api.Domain;

public static class MenuCatalog
{
    public static readonly IReadOnlyList<MenuItem> Items = new List<MenuItem>
    {
        new() { Id = "x-burger", Name = "X Burger", Price = 5.00m, Type = MenuItemType.Sandwich },
        new() { Id = "x-egg", Name = "X Egg", Price = 4.50m, Type = MenuItemType.Sandwich },
        new() { Id = "x-bacon", Name = "X Bacon", Price = 7.00m, Type = MenuItemType.Sandwich },
        new() { Id = "fries", Name = "Batata Frita", Price = 2.00m, Type = MenuItemType.FrenchFries },
        new() { Id = "soda", Name = "Refrigerante", Price = 2.50m, Type = MenuItemType.Soda },
    };

    public static MenuItem? FindById(string id) =>
        Items.FirstOrDefault(i => i.Id == id);
}
