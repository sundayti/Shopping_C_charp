using MediatR;
using OrdersService.Application.DTOs;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Queries;

public class GetOrdersListHandler(IOrderRepository repository) : IRequestHandler<GetOrdersListQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersListQuery request, CancellationToken ct)
    {
        var orders = await repository.GetByUserIdAsync(request.UserId, ct);
        return orders.Select(o => new OrderDto(
            o.Id,
            o.UserId,
            o.Amount,
            o.Description,
            o.Status.ToString()
        ));
    }
}