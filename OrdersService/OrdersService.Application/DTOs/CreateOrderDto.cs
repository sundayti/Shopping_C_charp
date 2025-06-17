using System.ComponentModel.DataAnnotations;

namespace OrdersService.Application.DTOs;

/// <summary>
/// DTO для создания заказа.
/// </summary>
public record CreateOrderDto
{
    /// <summary>
    /// Идентификатор пользователя, от чьего имени создаётся заказ.
    /// </summary>
    [Required]
    public Guid UserId { get; init; }

    /// <summary>
    /// Сумма заказа. Должна быть > 0.
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
    public decimal Amount { get; init; }

    /// <summary>
    /// Описание заказа.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}