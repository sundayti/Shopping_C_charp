using PaymentsServer.Domain.Entities;

namespace PaymentsServer.Domain.Interfaces;

public interface IInboxRepository
{
    public Task<InboxMessage> GetById(Guid id);
    public Task AddAsync(InboxMessage message);
}