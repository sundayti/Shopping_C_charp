using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;
using OrdersService.Domain.ValueObjects;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Infrastructure.Repositories;

public class InboxRepository(OrdersDbContext context) : IInboxRepository
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