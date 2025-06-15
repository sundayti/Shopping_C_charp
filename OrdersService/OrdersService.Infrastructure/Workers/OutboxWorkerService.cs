using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrdersService.Domain.Interfaces;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Infrastructure.Workers;


public class OutboxWorkerService(
    IServiceScopeFactory scopeFactory,
    IProducer<string, string> producer)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();

            await repo.BeginTransactionAsync(stoppingToken);

            var pending = await repo.GetPendingAsync(batchSize: 1, stoppingToken);
            if (pending.Count == 0)
            {
                await repo.RollbackTransactionAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            var msg = pending[0];

            var kafkaMsg = new Message<string, string>
            {
                Key   = msg.Id.ToString(),
                Value = msg.Content
            };
            await producer.ProduceAsync(msg.Type, kafkaMsg, stoppingToken);

            repo.MarkAsSuccess(msg);
            await repo.SaveChangesAsync(stoppingToken);
            
            await repo.CommitTransactionAsync(stoppingToken);
        }
    }
}