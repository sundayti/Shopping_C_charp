using PaymentsServer.Domain.Entities;

namespace PaymentsServer.Domain.Interfaces;

public interface IInboxRepository
{
    Task<InboxMessage?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(InboxMessage message, CancellationToken ct = default);
    Task<List<InboxMessage>> GetReceivedAsync(int batchSize, CancellationToken ct = default);
    void MarkAsSuccess(InboxMessage message);
}