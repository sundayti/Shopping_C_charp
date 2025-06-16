using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Application.Commands;
using OrdersService.Application.DTOs;
using OrdersService.Application.Queries;

namespace OrdersService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
    {
        var command = new CreateOrderCommand(request.UserId, request.Amount, request.Description);
        var result = await mediator.Send(command);

        return Ok(new { result });
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetOrders(Guid userId)
    {
        var query = new GetOrdersListQuery(userId);
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("status/{orderId:guid}")]
    public async Task<IActionResult> GetOrderStatus(Guid userId, Guid orderId)
    {
        var query = new GetOrderStatusQuery(orderId);
        var result = await mediator.Send(query);
        var response = new
        {
            fileId = result.Status
        };
        return Ok(response);
    }
}
