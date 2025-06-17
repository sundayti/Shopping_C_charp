using PaymentsServer.Domain.Entities;

namespace PaymentsServer.Domain.Interfaces;

public interface IPaymentAccountRepository
{
    Task<PaymentAccount?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(PaymentAccount account, CancellationToken ct = default);
    void Update(PaymentAccount account);
}