using OrdersService.Domain.Entities;

namespace OrdersService.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default);
    void Add(Order order);
}