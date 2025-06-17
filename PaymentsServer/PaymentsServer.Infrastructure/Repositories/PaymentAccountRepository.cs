using PaymentsServer.Domain.Entities;
using PaymentsServer.Domain.Interfaces;
using PaymentsServer.Infrastructure.Persistence;

namespace PaymentsServer.Infrastructure.Repositories;

public class PaymentAccountRepository(PaymentAccountsDbContext context) : IPaymentAccountRepository
{
    public async Task<PaymentAccount?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await context.PaymentAccounts.FindAsync([userId], cancellationToken: ct);
    }

    public async Task AddAsync(PaymentAccount account, CancellationToken ct = default)
    {
        await context.PaymentAccounts.AddAsync(account, ct);
    }

    public void Update(PaymentAccount account)
    {
        context.PaymentAccounts.Update(account);
    }
}