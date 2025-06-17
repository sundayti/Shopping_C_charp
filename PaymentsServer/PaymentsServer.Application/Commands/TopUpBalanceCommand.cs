using MediatR;
using PaymentsServer.Application.Exception;
using PaymentsServer.Domain.Interfaces;

namespace PaymentsServer.Application.Commands;

public record TopUpBalanceCommand(Guid UserId, decimal Amount) : IRequest;

public class TopUpBalanceCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<TopUpBalanceCommand>
{
    public async Task Handle(TopUpBalanceCommand request, CancellationToken ct)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidTopUpAmountException("Amount must be greater than 0");
        }
        
        var account = await unitOfWork.PaymentAccounts.GetByIdAsync(request.UserId, ct);

        if (account == null)
        {
            throw new KeyNotFoundException($"User not found {request.UserId}");
        }
        
        account.Balance += request.Amount;
    }
}