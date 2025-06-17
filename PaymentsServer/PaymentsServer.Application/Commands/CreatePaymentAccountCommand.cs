using MediatR;
using PaymentsServer.Domain.Entities;
using PaymentsServer.Domain.Interfaces;

namespace PaymentsServer.Application.Commands;

public record CreatePaymentAccountCommand(Guid? UserId) : IRequest<Guid>;

public class CreatePaymentAccountCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreatePaymentAccountCommand, Guid>
{
    public async  Task<Guid> Handle(CreatePaymentAccountCommand request, CancellationToken ct)
    {
        var uid = request.UserId ?? Guid.NewGuid();
        var account = new PaymentAccount(uid);
        await unitOfWork.PaymentAccounts.AddAsync(account, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return uid;
    }
}