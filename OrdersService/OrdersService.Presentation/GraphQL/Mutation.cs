using System.Text.Json;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Presentation.GraphQL;

public class Mutation
{
    public async Task<Order> CreateOrder(OrderInput input, [Service] OrdersDbContext context)
    {
        // Получи userId и amount из входных данных (или из контекста, авторизации и т.д.)
        Guid userId = /* как-то получаешь userId, например, из текущего пользователя */;
        decimal amount = input.Amount; // Предположим, что у тебя есть amount в OrderInput

        var order = Order.Create(
            userId: userId,
            amount: amount,
            description: $"Order for product {input.ProductId} in quantity {input.Quantity}");

        context.Orders.Add(order);

        // Создание сообщения для Transactional Outbox
        var outboxMessage = new OutboxMessage
        {
            Type = "OrderCreated",
            Content = JsonSerializer.Serialize(order),
            CreatedAt = DateTime.UtcNow
        };
        context.OutboxMessages.Add(outboxMessage);

        await context.SaveChangesAsync();

        return order;
    }
}