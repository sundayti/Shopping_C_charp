using System.Text.Json.Serialization;

namespace OrdersService.Application.DTOs;

public class OutboxMessageDto
{
    [JsonPropertyName("order_id")]
    public Guid Id { get; init; }
    [JsonPropertyName("user_id")]
    public Guid UserId { get; init; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }
}