using System.Text.Json.Serialization;
using MediatR;
using PaymentsServer.Domain.Interfaces;
using PaymentsServer.Domain.ValueObjects;

namespace PaymentsServer.Application.Commands;
public record AccountNotFoundError(Guid UserId);
public record InsufficientFundsError(Guid UserId);
public record Success;

public record DebitAccountCommand(
    [property: JsonPropertyName("user_id")] Guid UserId,
    [property: JsonPropertyName("order_id")] Guid OrderId,
    [property: JsonPropertyName("amount")] decimal Amount) 
    : IRequest<OneOf.OneOf<OneOf.Types.Success, AccountNotFoundError, InsufficientFundsError>>;
    

public class DebitAccountCommandHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<DebitAccountCommand, OneOf.OneOf<OneOf.Types.Success, AccountNotFoundError, InsufficientFundsError>>
{
    public async Task<OneOf.OneOf<OneOf.Types.Success, AccountNotFoundError, InsufficientFundsError>> Handle(DebitAccountCommand request, CancellationToken ct)
    {
        var account = await unitOfWork.PaymentAccounts.GetByIdAsync(request.UserId, ct);
        if (account is null)
        {
            return new AccountNotFoundError(request.UserId);
        }

        try
        {
            account.Balance -= request.Amount;
            unitOfWork.PaymentAccounts.Update(account);
            
            return new OneOf.Types.Success();
        }
        catch (ArgumentOutOfRangeException)
        {
            return new InsufficientFundsError(request.UserId);
        }
    }
}