using OrdersService.Domain.Interfaces;

namespace OrdersService.Infrastructure.Persistence;

public class Commiter(OrdersDbContext dbContext) : ICommiter
{
    public Task<int> CommitAsync(CancellationToken ct = default)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}