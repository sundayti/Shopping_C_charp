using PaymentsServer.Domain.ValueObjects;

namespace PaymentsServer.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.InProgress;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}