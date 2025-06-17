namespace PaymentsServer.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IPaymentAccountRepository PaymentAccounts { get; }
    IInboxRepository InboxMessages { get; }
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}