using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Infrastructure.Workers;

public class OutboxWorker(
    IServiceScopeFactory scopeFactory,
    IProducer<string, string> producer,
    ILogger<OutboxWorker> logger) : BackgroundService
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
            logger.LogInformation("Kafka message produced: {Type} - {Content}", msg.Type, msg.Content);

            unitOfWork.OutboxMessages.MarkAsSuccess(msg);
            await unitOfWork.CommitTransactionAsync(ct);
        }
    }
}