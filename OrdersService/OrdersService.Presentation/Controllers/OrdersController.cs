using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Application.Commands;
using OrdersService.Application.DTOs;
using OrdersService.Application.Queries;

namespace OrdersService.Presentation.Controllers;

[ApiController]
[Route("api")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
    {
        var command = new CreateOrderCommand(request.UserId, request.Amount, request.Description);
        try
        {
            var result = await mediator.Send(command);
            return Ok(new { result });
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetOrders(Guid userId)
    {
        var query = new GetOrdersListQuery(userId);
        try
        {
            var result = await mediator.Send(query);
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("status/{orderId:guid}")]
    public async Task<IActionResult> GetOrderStatus(Guid orderId)
    {
        var query = new GetOrderStatusQuery(orderId);
        try
        {
            var result = await mediator.Send(query);
            var response = new
            {
                fileId = result.Status
            };
            return Ok(response);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occurred.");
        }
    }
}