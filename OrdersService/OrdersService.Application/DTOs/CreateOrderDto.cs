namespace OrdersService.Application.DTOs;

public record CreateOrderDto(Guid UserId, decimal Amount, string Description);