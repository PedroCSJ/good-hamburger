using FluentAssertions;
using GoodHamburger.Api.Domain;
using GoodHamburger.Api.Services;

namespace GoodHamburger.Tests;

public class DiscountRulesTests
{
    private static Order BuildOrder(params MenuItemType[] types)
    {
        var items = types.Select(t =>
        {
            var item = MenuCatalog.Items.First(i => i.Type == t);
            return new OrderItem
            {
                MenuItemId = item.Id,
                Name = item.Name,
                UnitPrice = item.Price,
                Type = t
            };
        }).ToList();

        var order = new Order { Items = items };
        DiscountCalculator.Apply(order);
        return order;
    }

    [Fact]
    public void SandwichFriesSoda_ShouldApply20PercentDiscount()
    {
        var order = BuildOrder(MenuItemType.Sandwich, MenuItemType.FrenchFries, MenuItemType.Soda);

        order.DiscountPercent.Should().Be(20);
        order.DiscountAmount.Should().Be(order.Subtotal * 0.20m);
        order.Total.Should().Be(order.Subtotal - order.DiscountAmount);
    }

    [Fact]
    public void SandwichSoda_ShouldApply15PercentDiscount()
    {
        var order = BuildOrder(MenuItemType.Sandwich, MenuItemType.Soda);

        order.DiscountPercent.Should().Be(15);
        order.DiscountAmount.Should().Be(Math.Round(order.Subtotal * 0.15m, 2));
        order.Total.Should().Be(order.Subtotal - order.DiscountAmount);
    }

    [Fact]
    public void SandwichFries_ShouldApply10PercentDiscount()
    {
        var order = BuildOrder(MenuItemType.Sandwich, MenuItemType.FrenchFries);

        order.DiscountPercent.Should().Be(10);
        order.DiscountAmount.Should().Be(order.Subtotal * 0.10m);
    }

    [Fact]
    public void SandwichOnly_ShouldHaveNoDiscount()
    {
        var order = BuildOrder(MenuItemType.Sandwich);

        order.DiscountPercent.Should().Be(0);
        order.DiscountAmount.Should().Be(0);
        order.Total.Should().Be(order.Subtotal);
    }

    [Fact]
    public void FullCombo_XBurger_ShouldCalculateValuesCorrectly()
    {
        // X Burger R$5,00 + Batata R$2,00 + Refri R$2,50 = R$9,50
        // 20% de desconto = R$1,90 → total R$7,60
        var xBurger = MenuCatalog.FindById("x-burger")!;
        var fries = MenuCatalog.FindById("fries")!;
        var soda = MenuCatalog.FindById("soda")!;

        var order = new Order
        {
            Items =
            [
                new() { MenuItemId = xBurger.Id, Name = xBurger.Name, UnitPrice = xBurger.Price, Type = xBurger.Type },
                new() { MenuItemId = fries.Id, Name = fries.Name, UnitPrice = fries.Price, Type = fries.Type },
                new() { MenuItemId = soda.Id, Name = soda.Name, UnitPrice = soda.Price, Type = soda.Type },
            ]
        };

        DiscountCalculator.Apply(order);

        order.Subtotal.Should().Be(9.50m);
        order.DiscountPercent.Should().Be(20);
        order.DiscountAmount.Should().Be(1.90m);
        order.Total.Should().Be(7.60m);
    }

    [Fact]
    public void SandwichSodaCombo_XBacon_ShouldCalculateValuesCorrectly()
    {
        // X Bacon R$7,00 + Refri R$2,50 = R$9,50
        // 15% de R$9,50 = R$1,425 → arredonda para R$1,42 → total R$8,08
        var xBacon = MenuCatalog.FindById("x-bacon")!;
        var soda = MenuCatalog.FindById("soda")!;

        var order = new Order
        {
            Items =
            [
                new() { MenuItemId = xBacon.Id, Name = xBacon.Name, UnitPrice = xBacon.Price, Type = xBacon.Type },
                new() { MenuItemId = soda.Id, Name = soda.Name, UnitPrice = soda.Price, Type = soda.Type },
            ]
        };

        DiscountCalculator.Apply(order);

        order.Subtotal.Should().Be(9.50m);
        order.DiscountPercent.Should().Be(15);
        order.DiscountAmount.Should().Be(1.42m);
        order.Total.Should().Be(8.08m);
    }
}
