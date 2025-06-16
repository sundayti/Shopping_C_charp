namespace OrdersService.Application.DTOs;

public record OrderDto(
    Guid Id,
    Guid UserId,
    decimal Amount,
    string Description,
    string Status
);