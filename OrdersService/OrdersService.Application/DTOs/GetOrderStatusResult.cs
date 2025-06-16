using OrdersService.Domain.ValueObjects;

namespace OrdersService.Application.DTOs;

public record GetOrderStatusResult(OrderStatus Status);