using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Infrastructure.Repositories;

public class OrderRepository(OrdersDbContext dbContext) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
    }
    
    public async Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public void Add(Order order)
    {
        dbContext.Orders.Add(order);
    }
}