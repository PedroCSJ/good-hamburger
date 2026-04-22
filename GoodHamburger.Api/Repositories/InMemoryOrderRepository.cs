using System.Collections.Concurrent;
using GoodHamburger.Api.Domain;

namespace GoodHamburger.Api.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _store = new();

    public Order Add(Order order)
    {
        _store[order.Id] = order;
        return order;
    }

    public Order? GetById(Guid id) =>
        _store.TryGetValue(id, out var order) ? order : null;

    public IReadOnlyList<Order> GetAll() =>
        _store.Values.OrderByDescending(o => o.CreatedAt).ToList();

    public Order Update(Order order)
    {
        _store[order.Id] = order;
        return order;
    }

    public bool Delete(Guid id) =>
        _store.TryRemove(id, out _);
}
