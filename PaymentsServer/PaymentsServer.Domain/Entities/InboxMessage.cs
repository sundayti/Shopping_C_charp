using PaymentsServer.Domain.ValueObjects;

namespace PaymentsServer.Domain.Entities;

public class InboxMessage
{
    public Guid Id { get; init; } 
    public string Type { get; init; } 
    public string Content { get; init; } 
    public InboxMessageStatus Status { get; set; } = InboxMessageStatus.Received;
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}