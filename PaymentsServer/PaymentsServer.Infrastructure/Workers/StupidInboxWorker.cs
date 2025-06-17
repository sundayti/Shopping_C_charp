using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentsServer.Application.Commands;
using PaymentsServer.Domain.Interfaces;
using PaymentsServer.Domain.Entities;

namespace PaymentsServer.Infrastructure.Workers;

public class StupidInboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<StupidInboxWorker> _logger;

    public StupidInboxWorker(
        IServiceScopeFactory scopeFactory,
        IConsumer<string, string> consumer,
        ILogger<StupidInboxWorker> logger
        )
    {
        _scopeFactory = scopeFactory;
        _consumer = consumer;
        _consumer.Subscribe("create-order-topic");
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Waiting for messages...");
                var cr = _consumer.Consume(TimeSpan.FromSeconds(1));
                if (cr == null || cr.Message == null)
                {
                    await Task.Delay(100, ct);
                    continue;
                }
                _logger.LogInformation("Got message Value: {Value}", cr.Message.Value);
                
                var msg = cr.Message.Value;
                var id = cr.Message.Key;
                if (!Guid.TryParse(id, out var guid))
                {
                    _logger.LogWarning("Invalid GUID: {Id}", id);
                    return;
                }
                if (msg != null)
                {
                    _logger.LogInformation("Got message Key: {Key}", cr.Message.Key);
                    _logger.LogInformation("Got message Value: {Value}", cr.Message.Value);
                    using var scope = _scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    
                    var command = new AddInboxMessageCommand(guid, "create-order-topic", msg);
                    await mediator.Send(command, ct);
                    
                    _consumer.Commit(cr);
                    _logger.LogInformation("Saved to inbox_messages, committing offset");
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