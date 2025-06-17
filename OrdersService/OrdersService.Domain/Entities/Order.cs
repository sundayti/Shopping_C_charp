using OrdersService.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace OrdersService.Domain.Entities;

public sealed class Order
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }
    [JsonPropertyName("user_id")]
    public Guid UserId { get; init; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }
    [JsonIgnore]
    public string Description { get; init; }
    [JsonIgnore]
    public OrderStatus Status { get; set; }
    [JsonIgnore]
    public DateTime CreatedAt { get; init; }
    
    private Order() { }

    private Order(Guid id, Guid userId, decimal amount, string description, OrderStatus status)
    {
        Id = id;
        UserId = userId;
        Amount = amount;
        Description = description;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public static Order Create(Guid userId, decimal amount, string description)
    {
        return new Order(Guid.NewGuid(), userId, amount, description, OrderStatus.New);
    }
}