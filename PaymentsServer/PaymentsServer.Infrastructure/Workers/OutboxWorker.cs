using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentsServer.Domain.Interfaces;

namespace PaymentsServer.Infrastructure.Workers;

public class OutboxWorkerService(
    IServiceScopeFactory scopeFactory,
    IProducer<string, string> producer)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await unitOfWork.BeginTransactionAsync(ct);

            var pending = await unitOfWork.OutboxMessages.GetPendingAsync(batchSize: 1, ct);
            if (pending.Count == 0)
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
                continue;
            }

            var msg = pending[0];

            var kafkaMsg = new Message<string, string>
            {
                Key   = msg.Id.ToString(),
                Value = msg.Content
            };
            await producer.ProduceAsync(msg.Type, kafkaMsg, ct);

            unitOfWork.OutboxMessages.MarkAsSuccess(msg);
            
            await unitOfWork.CommitTransactionAsync(ct);
        }
    }
}