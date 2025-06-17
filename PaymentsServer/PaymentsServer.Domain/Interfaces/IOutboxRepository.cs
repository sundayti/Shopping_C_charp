using PaymentsServer.Domain.Entities;

namespace PaymentsServer.Domain.Interfaces;

public interface IOutboxRepository
{
    Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default);
    void MarkAsSuccess(OutboxMessage msg);
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);
}