using System.Text.Json;
using MediatR;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Commands;

public class CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IOutboxMessageRepository outboxMessageRepository,
        ICommiter commiter
    ) : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = Order.Create(request.UserId, request.Amount, request.Description);

        var content = JsonSerializer.Serialize(order);
        var outboxMessage = new OutboxMessage
        {
            Type = "orderCreated",
            Content = content
        };
        orderRepository.Add(order);
        outboxMessageRepository.Add(outboxMessage);

        await commiter.CommitAsync(ct);
        
        return order.Id;
    }
}