namespace OrdersService.Domain.Interfaces;

public interface ICommiter
{
    Task<int> CommitAsync(CancellationToken ct = default);
}