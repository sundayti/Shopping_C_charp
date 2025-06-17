using Microsoft.EntityFrameworkCore.Storage;
using PaymentsServer.Domain.Interfaces;
using PaymentsServer.Infrastructure.Repositories;

namespace PaymentsServer.Infrastructure.Persistence;

public sealed class UnitOfWork(PaymentAccountsDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IPaymentAccountRepository PaymentAccounts { get; } = new PaymentAccountRepository(context);
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