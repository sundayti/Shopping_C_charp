using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentsServer.Application.Commands;
using PaymentsServer.Application.Exception;
using PaymentsServer.Application.Queries;

namespace PaymentsServer.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentAccountsController(IMediator mediator) : ControllerBase
{
    [HttpPost("{userId:guid}")]
    public async Task<IActionResult> CreateAccountWithUid(Guid userId)
    {
        var command = new CreatePaymentAccountCommand(userId);
        try
        {
            var result =await mediator.Send(command);
            var response = new
            {
                accountId = result
            };
            return Ok(response);
        }
        catch (DbUpdateException)
        {
            return Conflict("Bank account already exists");
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount()
    {
        var command = new CreatePaymentAccountCommand(null);
        try
        {
            var result =await mediator.Send(command);
            var response = new
            {
                accountId = result
            };
            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance([FromBody] Guid userId)
    {
        var query = new GetBalanceQuery(userId);
        try
        {
            var result = await mediator.Send(query);
            var response = new
            {
                balance = result
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

    [HttpPost("deposit")]
    public async Task<IActionResult> UpdateBalance([FromBody] Guid userId, [FromBody] decimal amount)
    {
        var command = new TopUpBalanceCommand(userId, amount);
        try
        {
            var result = await mediator.Send(command);
            var response = new
            {
                balance = result
            };
            return Ok(response);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (InvalidTopUpAmountException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occurred.");
        }
    }
}