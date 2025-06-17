using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentsServer.Application.Commands;
using PaymentsServer.Application.Exception;
using PaymentsServer.Application.Queries;

namespace PaymentsServer.Presentation.Controllers;
/// <summary>
/// Управление платёжными аккаунтами пользователей.
/// </summary>
[ApiController]
[Route("api")]
public class PaymentAccountsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Создаёт платёжный аккаунт для указанного пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>
    /// 200 OK + { accountId = Guid } — идентификатор созданного аккаунта;  
    /// 409 Conflict — если аккаунт для пользователя уже существует;  
    /// 500 Internal Server Error — при прочих ошибках.
    /// </returns>
    [HttpPost("{userId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAccountWithUid(Guid userId)
    {
        var command = new CreatePaymentAccountCommand(userId);
        try
        {
            var result = await mediator.Send(command);
            return Ok(new { accountId = result });
        }
        catch (DbUpdateException)
        {
            return Conflict("Bank account already exists");
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred.");
        }
    }

    /// <summary>
    /// Создаёт новый платёжный аккаунт без предварительного указания пользователя.
    /// </summary>
    /// <remarks>
    /// Внутри команда сама генерирует или связывает пользователя с аккаунтом.
    /// </remarks>
    /// <returns>
    /// 200 OK + { accountId = Guid } — идентификатор созданного аккаунта;  
    /// 500 Internal Server Error — при ошибке на сервере.
    /// </returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAccount()
    {
        var command = new CreatePaymentAccountCommand(null);
        try
        {
            var result = await mediator.Send(command);
            return Ok(new { accountId = result });
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred.");
        }
    }

    /// <summary>
    /// Возвращает текущий баланс платёжного аккаунта пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>
    /// 200 OK + { balance = decimal } — текущий баланс;  
    /// 404 Not Found — если аккаунт не найден;  
    /// 500 Internal Server Error — при прочих ошибках.
    /// </returns>
    [HttpGet("balance")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBalance([FromQuery] Guid userId)
    {
        var query = new GetBalanceQuery(userId);
        try
        {
            var result = await mediator.Send(query);
            return Ok(new { balance = result });
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

    /// <summary>
    /// Пополняет баланс платёжного аккаунта.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="amount">Сумма пополнения (должна быть >0).</param>
    /// <returns>
    /// 200 OK + { balance = decimal } — обновлённый баланс;  
    /// 404 Not Found — если аккаунт не найден;  
    /// 400 Bad Request — неверная сумма пополнения;  
    /// 500 Internal Server Error — при прочих ошибках.
    /// </returns>
    [HttpPost("deposit")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateBalance(
        [FromQuery] Guid userId,
        [FromQuery] decimal amount)
    {
        var command = new TopUpBalanceCommand(userId, amount);
        try
        {
            var result = await mediator.Send(command);
            return Ok(new { balance = result });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (InvalidTopUpAmountException e)
        {
            return BadRequest(e.Message);
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred.");
        }
    }
}