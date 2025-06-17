using Microsoft.EntityFrameworkCore.Storage;
using OrdersService.Domain.Interfaces;
using OrdersService.Infrastructure.Repositories;

namespace OrdersService.Infrastructure.Persistence;

public sealed class UnitOfWork(OrdersDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IOrderRepository Orders { get; } = new OrderRepository(context);
    public IInboxRepository InboxMessages { get; } = new InboxRepository(context);
    public IOutboxRepository OutboxMessages { get; } = new OutboxRepository(context);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            await SaveChangesAsync(ct);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(ct);
            }
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
    }
    
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return context.SaveChangesAsync(ct);
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }
}