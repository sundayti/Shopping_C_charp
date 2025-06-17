using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Application.Commands;
using OrdersService.Application.DTOs;
using OrdersService.Application.Queries;

namespace OrdersService.Presentation.Controllers;

/// <summary>
/// Контроллер для работы с заказами.
/// </summary>
[ApiController]
[Route("api")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Создаёт новый заказ и запускает асинхронный процесс оплаты.
    /// </summary>
    /// <param name="request">Параметры заказа: идентификатор пользователя, сумма и описание.</param>
    /// <returns>
    /// 200 OK + { result = Guid } — где result — сгенерированный идентификатор заказа;  
    /// 500 Internal Server Error — при ошибке на сервере.
    /// </returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
    {
        var command = new CreateOrderCommand(request.UserId, request.Amount, request.Description);
        try
        {
            var result = await mediator.Send(command);
            return Ok(new { result });
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred.");
        }
    }

    /// <summary>
    /// Возвращает список всех заказов пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>
    /// 200 OK + массив объектов OrderDto;  
    /// 500 Internal Server Error — при ошибке на сервере.
    /// </returns>
    [HttpGet("{userId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrders(Guid userId)
    {
        var query = new GetOrdersListQuery(userId);
        try
        {
            var result = await mediator.Send(query);
            return Ok(result);
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred.");
        }
    }

    /// <summary>
    /// Возвращает статус конкретного заказа (например, идентификатор файла с деталями).
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <returns>
    /// 200 OK + { fileId = string } — если заказ найден;  
    /// 404 Not Found — если заказ не существует;  
    /// 500 Internal Server Error — при ошибке на сервере.
    /// </returns>
    [HttpGet("status/{orderId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderStatus(Guid orderId)
    {
        var query = new GetOrderStatusQuery(orderId);
        try
        {
            var result = await mediator.Send(query);
            return Ok(new { fileId = result.Status });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred.");
        }
    }
}