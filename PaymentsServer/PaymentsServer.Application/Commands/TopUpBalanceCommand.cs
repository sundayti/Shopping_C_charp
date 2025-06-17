using MediatR;
using PaymentsServer.Application.Exception;
using PaymentsServer.Domain.Interfaces;

namespace PaymentsServer.Application.Commands;

public record TopUpBalanceCommand(Guid UserId, decimal Amount) : IRequest<Decimal>;

public class TopUpBalanceCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<TopUpBalanceCommand, Decimal>
{
    public async Task<Decimal> Handle(TopUpBalanceCommand request, CancellationToken ct)
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
        await unitOfWork.SaveChangesAsync(ct);
        return account.Balance.Value;
    }
}