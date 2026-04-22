using GoodHamburger.Api.Domain;
using GoodHamburger.Api.DTOs;

namespace GoodHamburger.Api.Services;

public interface IOrderService
{
    OrderResponse Create(CreateOrderRequest request);
    OrderResponse GetById(Guid id);
    IReadOnlyList<OrderResponse> GetAll();
    OrderResponse Update(Guid id, UpdateOrderRequest request);
    void Delete(Guid id);
}
