using Microsoft.EntityFrameworkCore;
using PaymentsServer.Domain.Entities;
using PaymentsServer.Domain.Interfaces;
using PaymentsServer.Domain.ValueObjects;
using PaymentsServer.Infrastructure.Persistence;

namespace PaymentsServer.Infrastructure.Repositories;

public class InboxRepository(PaymentAccountsDbContext context) : IInboxRepository
{
    public async Task<InboxMessage?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.InboxMessages.FindAsync([id], cancellationToken: ct);
    }

    public async Task AddAsync(InboxMessage message, CancellationToken ct = default)
    {
        await context.InboxMessages.AddAsync(message, ct);
    }

    public async Task<List<InboxMessage>> GetReceivedAsync(int batchSize, CancellationToken ct = default)
    {
        return await context.InboxMessages
            .Where(m => m.Status == InboxMessageStatus.Received)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public void MarkAsSuccess(InboxMessage message)
    {
        message.Status = InboxMessageStatus.Success;
    }
}