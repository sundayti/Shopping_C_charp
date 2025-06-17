using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;
using OrdersService.Domain.ValueObjects;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Infrastructure.Repositories;

public class OutboxRepository(OrdersDbContext dbContext) : IOutboxRepository
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
        if (msg == null) throw new ArgumentNullException(nameof(msg));
        msg.Status = OutboxMessageStatus.Success;
    }

    public void Add(OutboxMessage msg)
    {
        if (msg == null) throw new ArgumentNullException(nameof(msg));
        dbContext.OutboxMessages.Add(msg);
    }
    
    public async Task AddAsync(OutboxMessage message, CancellationToken ct = default)
    {
        await dbContext.OutboxMessages.AddAsync(message, ct);
    }
}