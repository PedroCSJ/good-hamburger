using GoodHamburger.Api.Domain;
using GoodHamburger.Api.DTOs;
using GoodHamburger.Api.Repositories;

namespace GoodHamburger.Api.Services;

public class OrderService(IOrderRepository repository) : IOrderService
{
    public OrderResponse Create(CreateOrderRequest request)
    {
        var items = ResolveItems(request.ItemIds);
        var order = new Order { Items = items };
        DiscountCalculator.Apply(order);
        repository.Add(order);
        return ToResponse(order);
    }

    public OrderResponse GetById(Guid id)
    {
        var order = repository.GetById(id)
            ?? throw new KeyNotFoundException($"Pedido {id} não encontrado.");
        return ToResponse(order);
    }

    public IReadOnlyList<OrderResponse> GetAll() =>
        repository.GetAll().Select(ToResponse).ToList();

    public OrderResponse Update(Guid id, UpdateOrderRequest request)
    {
        var order = repository.GetById(id)
            ?? throw new KeyNotFoundException($"Pedido {id} não encontrado.");

        order.Items = ResolveItems(request.ItemIds);
        order.UpdatedAt = DateTime.UtcNow;
        DiscountCalculator.Apply(order);
        repository.Update(order);
        return ToResponse(order);
    }

    public void Delete(Guid id)
    {
        var deleted = repository.Delete(id);
        if (!deleted)
            throw new KeyNotFoundException($"Pedido {id} não encontrado.");
    }

    private static List<OrderItem> ResolveItems(List<string> itemIds)
    {
        if (itemIds.Count != itemIds.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            throw new InvalidOperationException("O pedido contém itens duplicados.");

        var items = new List<OrderItem>();

        foreach (var id in itemIds)
        {
            var menuItem = MenuCatalog.FindById(id)
                ?? throw new ArgumentException($"Item '{id}' não encontrado no cardápio.");

            items.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                Name = menuItem.Name,
                UnitPrice = menuItem.Price,
                Type = menuItem.Type
            });
        }

        ValidateItemTypeConstraints(items);

        return items;
    }

    private static void ValidateItemTypeConstraints(List<OrderItem> items)
    {
        var sandwiches = items.Count(i => i.Type == MenuItemType.Sandwich);
        var fries = items.Count(i => i.Type == MenuItemType.FrenchFries);
        var sodas = items.Count(i => i.Type == MenuItemType.Soda);

        if (sandwiches > 1)
            throw new InvalidOperationException("O pedido só pode ter um sanduíche.");
        if (fries > 1)
            throw new InvalidOperationException("O pedido só pode ter uma batata frita.");
        if (sodas > 1)
            throw new InvalidOperationException("O pedido só pode ter um refrigerante.");
        if (sandwiches == 0)
            throw new InvalidOperationException("O pedido deve ter pelo menos um sanduíche.");
    }

    private static OrderResponse ToResponse(Order order) => new()
    {
        Id = order.Id,
        Items = order.Items.Select(i => new OrderItemResponse
        {
            MenuItemId = i.MenuItemId,
            Name = i.Name,
            UnitPrice = i.UnitPrice
        }).ToList(),
        Subtotal = order.Subtotal,
        DiscountPercent = order.DiscountPercent,
        DiscountAmount = order.DiscountAmount,
        Total = order.Total,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt
    };
}
