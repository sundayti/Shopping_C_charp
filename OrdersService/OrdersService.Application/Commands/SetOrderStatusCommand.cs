using System.Text.Json.Serialization;
using MediatR;
using OrdersService.Domain.Interfaces;
using OrdersService.Domain.ValueObjects;

namespace OrdersService.Application.Commands;
public record OrderNotFoundError(Guid OrderId);
public record Success;

public record SetOrderStatusCommand(
    [property: JsonPropertyName("order_id")] Guid OrderId,
    [property: JsonPropertyName("is_success")] bool IsSuccess) 
    : IRequest<OneOf.OneOf<Success, OrderNotFoundError>>;
    

public class SetOrderStatusCommandHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<SetOrderStatusCommand, OneOf.OneOf<Success, OrderNotFoundError>>
{
    public async Task<OneOf.OneOf<Success, OrderNotFoundError>> Handle(SetOrderStatusCommand request, CancellationToken ct)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(request.OrderId, ct);

        if (order is null)
        {
            return new OrderNotFoundError(request.OrderId);
        }

        if (order.Status == OrderStatus.Finished || order.Status == OrderStatus.Cancelled)
        {
            return new Success(); 
        }

        order.Status = request.IsSuccess ? OrderStatus.Finished : OrderStatus.Cancelled;
        unitOfWork.Orders.Update(order);
        
        await unitOfWork.SaveChangesAsync(ct);
        
        return new Success();
    }
}