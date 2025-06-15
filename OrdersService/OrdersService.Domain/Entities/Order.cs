using OrdersService.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace OrdersService.Domain.Entities;

public sealed class Order
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    [JsonIgnore]
    public string Description { get; set; }
    [JsonIgnore]
    public OrderStatus Status { get; set; }
    [JsonIgnore]
    public DateTime CreatedAt { get; set; }
    
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
        return new Order(Guid.NewGuid(), userId, amount, description, OrderStatus.Pending);
    }
}