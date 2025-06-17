using Microsoft.EntityFrameworkCore;
using PaymentsServer.Domain.Entities;
using PaymentsServer.Domain.Interfaces;
using PaymentsServer.Domain.ValueObjects;
using PaymentsServer.Infrastructure.Persistence;

namespace PaymentsServer.Infrastructure.Repositories;

public class OutboxRepository(PaymentAccountsDbContext dbContext) : IOutboxRepository
{
    public async Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default)
    {
        return await dbContext.OutboxMessages
            .Where(x => x.Status == OutboxMessageStatus.InProgress)
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public void MarkAsSuccess(OutboxMessage msg)
    {
        msg.Status = OutboxMessageStatus.Success;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken ct = default)
    {
        await dbContext.OutboxMessages.AddAsync(message, ct);
    }
}