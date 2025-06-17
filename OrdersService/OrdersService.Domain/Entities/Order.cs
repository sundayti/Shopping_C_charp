using OrdersService.Domain.ValueObjects;

namespace OrdersService.Domain.Entities;

public sealed class Order
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public string Description { get; init; }
    public OrderStatus Status { get; set; }
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