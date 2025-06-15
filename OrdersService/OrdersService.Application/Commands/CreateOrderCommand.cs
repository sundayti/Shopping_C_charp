using MediatR;

namespace OrdersService.Application.Commands;

public record CreateOrderCommand(Guid UserId, decimal Amount, string Description) : IRequest<Guid>;