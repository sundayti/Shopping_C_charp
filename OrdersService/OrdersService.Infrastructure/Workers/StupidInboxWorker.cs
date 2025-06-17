using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrdersService.Application.Commands;

namespace OrdersService.Infrastructure.Workers;

public class StupidInboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;

    public StupidInboxWorker(
        IServiceScopeFactory scopeFactory,
        IConsumer<string, string> consumer
        )
    {
        _scopeFactory = scopeFactory;
        _consumer = consumer;
        _consumer.Subscribe("close-order-topic");
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var cr = _consumer.Consume(TimeSpan.FromSeconds(1));
                if (cr == null || cr.Message == null)
                {
                    await Task.Delay(100, ct);
                    continue;
                }
                
                var msg = cr.Message.Value;
                var id = cr.Message.Key;
                if (!Guid.TryParse(id, out var guid))
                {
                    return;
                }

                if (msg != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    
                    var command = new AddInboxMessageCommand(guid, "close-order-topic", msg);
                    await mediator.Send(command, ct);
                    
                    _consumer.Commit(cr);
                }
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
            await Task.Delay(100, ct);
        }

        _consumer.Close();
    }
}
