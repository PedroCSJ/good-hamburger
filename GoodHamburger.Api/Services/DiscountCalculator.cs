using GoodHamburger.Api.Domain;

namespace GoodHamburger.Api.Services;

public static class DiscountCalculator
{
    public static void Apply(Order order)
    {
        var hasSandwich = order.Items.Any(i => i.Type == MenuItemType.Sandwich);
        var hasFries = order.Items.Any(i => i.Type == MenuItemType.FrenchFries);
        var hasSoda = order.Items.Any(i => i.Type == MenuItemType.Soda);

        order.DiscountPercent = (hasSandwich, hasFries, hasSoda) switch
        {
            (true, true, true) => 20,
            (true, false, true) => 15,
            (true, true, false) => 10,
            _ => 0
        };

        order.Subtotal = order.Items.Sum(i => i.UnitPrice);
        order.DiscountAmount = Math.Round(order.Subtotal * order.DiscountPercent / 100m, 2);
        order.Total = order.Subtotal - order.DiscountAmount;
    }
}
