using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;
using OrdersService.Domain.ValueObjects;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Infrastructure.Repositories;

public class OutboxMessageRepository(OrdersDbContext dbContext) : IOutboxMessageRepository
{
    private readonly OrdersDbContext _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private IDbContextTransaction?   _tx;

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _tx = await _db.Database.BeginTransactionAsync(ct);
    }

    public async Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default)
    {
        return await _db.OutboxMessages
            .Where(x => x.Status == OutboxMessageStatus.InProgress)
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public void MarkAsSuccess(OutboxMessage msg)
    {
        if (msg == null) throw new ArgumentNullException(nameof(msg));
        msg.Status = OutboxMessageStatus.Success;
    }

    public void Add(OutboxMessage msg)
    {
        if (msg == null) throw new ArgumentNullException(nameof(msg));
        _db.OutboxMessages.Add(msg);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_tx == null) throw new InvalidOperationException("Transaction has not been started.");
        await _tx.CommitAsync(ct);
        await _tx.DisposeAsync();
        _tx = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_tx != null)
        {
            await _tx.RollbackAsync(ct);
            await _tx.DisposeAsync();
            _tx = null;
        }
    }
}