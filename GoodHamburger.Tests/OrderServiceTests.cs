using FluentAssertions;
using GoodHamburger.Api.DTOs;
using GoodHamburger.Api.Repositories;
using GoodHamburger.Api.Services;

namespace GoodHamburger.Tests;

public class OrderServiceTests
{
    private static OrderService BuildService() =>
        new(new InMemoryOrderRepository());

    [Fact]
    public void Create_ValidOrder_ShouldReturnOrderWithId()
    {
        var service = BuildService();
        var request = new CreateOrderRequest { ItemIds = ["x-burger"] };

        var result = service.Create(request);

        result.Id.Should().NotBeEmpty();
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("X Burger");
        result.Total.Should().Be(5.00m);
    }

    [Fact]
    public void Create_WithDuplicateItemId_ShouldThrowInvalidOperation()
    {
        var service = BuildService();
        var request = new CreateOrderRequest { ItemIds = ["x-burger", "x-burger"] };

        var act = () => service.Create(request);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*duplicados*");
    }

    [Fact]
    public void Create_WithUnknownItemId_ShouldThrowArgumentException()
    {
        var service = BuildService();
        var request = new CreateOrderRequest { ItemIds = ["item-inexistente"] };

        var act = () => service.Create(request);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*não encontrado*");
    }

    [Fact]
    public void Create_WithTwoSandwiches_ShouldThrowInvalidOperation()
    {
        var service = BuildService();
        var request = new CreateOrderRequest { ItemIds = ["x-burger", "x-bacon"] };

        var act = () => service.Create(request);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*sanduíche*");
    }

    [Fact]
    public void Create_WithoutSandwich_ShouldThrowInvalidOperation()
    {
        var service = BuildService();
        var request = new CreateOrderRequest { ItemIds = ["fries", "soda"] };

        var act = () => service.Create(request);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*sanduíche*");
    }

    [Fact]
    public void GetById_ExistingOrder_ShouldReturnIt()
    {
        var service = BuildService();
        var created = service.Create(new CreateOrderRequest { ItemIds = ["x-egg"] });

        var found = service.GetById(created.Id);

        found.Id.Should().Be(created.Id);
    }

    [Fact]
    public void GetById_NonExistingOrder_ShouldThrowKeyNotFound()
    {
        var service = BuildService();

        var act = () => service.GetById(Guid.NewGuid());

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void GetAll_AfterCreatingTwoOrders_ShouldReturnBoth()
    {
        var service = BuildService();
        service.Create(new CreateOrderRequest { ItemIds = ["x-burger"] });
        service.Create(new CreateOrderRequest { ItemIds = ["x-egg", "fries"] });

        var all = service.GetAll();

        all.Should().HaveCount(2);
    }

    [Fact]
    public void Update_ExistingOrder_ShouldRecalculateDiscount()
    {
        var service = BuildService();
        var order = service.Create(new CreateOrderRequest { ItemIds = ["x-burger"] });

        var updated = service.Update(order.Id, new UpdateOrderRequest { ItemIds = ["x-burger", "soda"] });

        updated.DiscountPercent.Should().Be(15);
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_NonExistingOrder_ShouldThrowKeyNotFound()
    {
        var service = BuildService();

        var act = () => service.Update(Guid.NewGuid(), new UpdateOrderRequest { ItemIds = ["x-burger"] });

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Delete_ExistingOrder_ShouldRemoveIt()
    {
        var service = BuildService();
        var order = service.Create(new CreateOrderRequest { ItemIds = ["x-bacon"] });

        service.Delete(order.Id);

        var act = () => service.GetById(order.Id);
        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Delete_NonExistingOrder_ShouldThrowKeyNotFound()
    {
        var service = BuildService();

        var act = () => service.Delete(Guid.NewGuid());

        act.Should().Throw<KeyNotFoundException>();
    }
}
