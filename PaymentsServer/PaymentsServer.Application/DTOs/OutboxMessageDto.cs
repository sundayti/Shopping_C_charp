using System.Text.Json.Serialization;

namespace PaymentsServer.Application.DTOs;

public class OutboxMessageDto
{
    [JsonPropertyName("order_id")]
    public Guid OrderId { get; init; }
    [JsonPropertyName("is_success")]
    public bool IsSuccess { get; init; }
}