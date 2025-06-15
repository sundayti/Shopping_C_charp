using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Presentation.GraphQL;

public class Query
{
    public async Task<IEnumerable<Order>> GetOrders([Service] OrdersDbContext context)
    {
        return await context.Orders.ToListAsync();
    }
}