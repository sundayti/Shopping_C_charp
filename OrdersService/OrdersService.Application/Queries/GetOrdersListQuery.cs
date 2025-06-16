using MediatR;
using OrdersService.Application.DTOs;

namespace OrdersService.Application.Queries;

public record GetOrdersListQuery(Guid UserId) : IRequest<IEnumerable<OrderDto>>;