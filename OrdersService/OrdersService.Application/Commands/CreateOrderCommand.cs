using System.Text.Json;
using MediatR;
using OrdersService.Application.DTOs;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Commands;

public record CreateOrderCommand(Guid UserId, decimal Amount, string Description) : IRequest<Guid>;

public class CreateOrderCommandHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = Order.Create(request.UserId, request.Amount, request.Description);
        
        var content = JsonSerializer.Serialize(new OutboxMessageDto
        {
            Id = order.Id,
            Amount = order.Amount,
            UserId = order.UserId
        });
        var outboxMessage = new OutboxMessage
        {
            Type = "create-order-topic",
            Content = content
        };

        await unitOfWork.BeginTransactionAsync(ct);
        await unitOfWork.Orders.AddAsync(order, ct);
        await unitOfWork.OutboxMessages.AddAsync(outboxMessage, ct);
        await unitOfWork.CommitTransactionAsync(ct);
        
        return order.Id;
    }
}