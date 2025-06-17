using MediatR;
using OrdersService.Application.DTOs;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Queries;

public record GetOrderStatusQuery(Guid OrderId) : IRequest<GetOrderStatusResult>;

public class GetOrderStatusHandler(IOrderRepository repository) : IRequestHandler<GetOrderStatusQuery, GetOrderStatusResult>
{
    public async Task<GetOrderStatusResult> Handle(GetOrderStatusQuery request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with Id {request.OrderId} was not found.");
        }
        var response = new GetOrderStatusResult(order.Status);
        return response;
    }
}