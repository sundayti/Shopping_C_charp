using MediatR;
using OrdersService.Application.DTOs;

namespace OrdersService.Application.Queries;

public record GetOrderStatusQuery(Guid OrderId) : IRequest<GetOrderStatusResult>;