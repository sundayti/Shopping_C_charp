using MediatR;
using PaymentsServer.Domain.Interfaces;

namespace PaymentsServer.Application.Queries;

public record GetBalanceQuery(Guid UserId) : IRequest<decimal>;

public class GetBalanceQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetBalanceQuery, decimal>
{
    public async Task<decimal> Handle(GetBalanceQuery request, CancellationToken ct)
    {
        var user = await unitOfWork.PaymentAccounts.GetByIdAsync(request.UserId, ct);

        if (user == null)
        {
            throw new KeyNotFoundException($"User not found {request.UserId}");
        }

        return user.Balance.Value;
    }
}