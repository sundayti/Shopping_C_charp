using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrdersService.Application.Commands;
using OrdersService.Application.Queries;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Infrastructure.Workers;

public class CleverInboxWorker(IServiceScopeFactory scopeFactory, ILogger<CleverInboxWorker> logger)
    : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromSeconds(5); 
    private const int BatchSize = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Inbox Processor Worker is starting.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessages(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unhandled exception occurred in InboxProcessorWorker.");
            }
            
            await Task.Delay(_period, stoppingToken);
        }
        
        logger.LogInformation("Inbox Processor Worker is stopping.");
    }
    
     private async Task ProcessPendingMessages(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var messages = await mediator.Send(new GetPendingInboxMessagesQuery(BatchSize), ct);
        if (!messages.Any()) return;

        logger.LogInformation("Found {Count} pending messages to process.", messages.Count);

        foreach (var message in messages)
        {
            await unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var command = JsonSerializer.Deserialize<SetOrderStatusCommand>(message.Content);
                if (command is null) 
                {
                    logger.LogError("Wrong content.");
                    continue;
                }

                var result = await mediator.Send(command, ct);
                
                switch (result.Value)
                {
                    case Success:
                        unitOfWork.InboxMessages.MarkAsSuccess(message);
                        logger.LogInformation("Success {OrderId}", command.OrderId);
                        break;
                    case OrderNotFoundError:
                        unitOfWork.InboxMessages.MarkAsFailed(message);
                        logger.LogError("Order {OrderId} not found", command.OrderId);
                        break;
                    default:
                        unitOfWork.InboxMessages.MarkAsFailed(message);
                        logger.LogError("Unknown error for order {OrderId}", command.OrderId);
                        break;
                }
                
                await unitOfWork.CommitTransactionAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "System failure on message {MessageId}. Rolling back.", message.Id);
                await unitOfWork.RollbackTransactionAsync(ct);
            }
        }
    }
}