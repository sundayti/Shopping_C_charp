using PaymentsServer.Domain.Interfaces;

namespace PaymentsServer.Infrastructure.Persistence;

public class Commiter(PaymentAccountsDbContext dbContext) : ICommiter
{
    public Task<int> CommitAsync(CancellationToken ct = default)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}