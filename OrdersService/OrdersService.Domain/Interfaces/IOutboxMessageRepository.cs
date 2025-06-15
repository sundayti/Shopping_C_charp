using OrdersService.Domain.Entities;

namespace OrdersService.Domain.Interfaces;

public interface IOutboxMessageRepository
{
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default);
    void MarkAsSuccess(OutboxMessage msg);
    void Add(OutboxMessage msg);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}

