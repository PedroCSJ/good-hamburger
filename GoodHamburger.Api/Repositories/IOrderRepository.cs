using GoodHamburger.Api.Domain;

namespace GoodHamburger.Api.Repositories;

public interface IOrderRepository
{
    Order Add(Order order);
    Order? GetById(Guid id);
    IReadOnlyList<Order> GetAll();
    Order Update(Order order);
    bool Delete(Guid id);
}
