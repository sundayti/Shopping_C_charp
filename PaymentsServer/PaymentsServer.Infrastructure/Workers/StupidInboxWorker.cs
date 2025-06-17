using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentsServer.Domain.Interfaces;
using PaymentsServer.Domain.Entities;

namespace PaymentsServer.Infrastructure.Workers;

public class StupidInboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;

    public StupidInboxWorker(
        IServiceScopeFactory scopeFactory,
        IConsumer<string, string> consumer)
    {
        _scopeFactory = scopeFactory;
        _consumer = consumer;
        _consumer.Subscribe("create-order-topic");
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var cr = _consumer.Consume(ct);

                var msg = System.Text.Json.JsonSerializer.Deserialize<InboxKafkaMessage>(cr.Message.Value);

                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IInboxRepository>();

                if (msg != null)
                    await repo.AddAsync(new InboxMessage
                    {
                        Id = msg.Id,
                        Type = msg.Type,
                        Content = msg.Content
                    }, ct);

                _consumer.Commit(cr);
            }
            catch (ConsumeException ex)
            {
                Console.WriteLine($"Kafka consume error: {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Processing error: {ex.Message}");
            }
        }

        _consumer.Close();
    }
}

public record InboxKafkaMessage(Guid Id, string Type, string Content);