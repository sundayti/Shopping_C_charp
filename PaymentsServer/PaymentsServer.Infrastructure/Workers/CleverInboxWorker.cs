using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentsServer.Application.Commands;
using PaymentsServer.Application.Queries;
using PaymentsServer.Domain.Interfaces;

namespace PaymentsServer.Infrastructure.Workers;


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
        
        // 1. Получаем пачку через Query
        var messages = await mediator.Send(new GetPendingInboxMessagesQuery(BatchSize), ct);
        if (!messages.Any()) return;

        logger.LogInformation("Found {Count} pending messages to process.", messages.Count);

        foreach (var message in messages)
        {
            await unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var command = JsonSerializer.Deserialize<DebitAccountCommand>(message.Content);
                if (command is null) 
                {
                    // TODO: обработка ошибки десериализации 
                    continue;
                }

                var result = await mediator.Send(command, ct);

                switch (result.Value)
                {
                    case Success:
                        unitOfWork.InboxMessages.MarkAsSuccess(message);
                        logger.LogInformation("Success {OrderId}", command.OrderId);
                        break;
                    case AccountNotFoundError:
                        unitOfWork.InboxMessages.MarkAsFailed(message);
                        logger.LogError("Account {UserId} not found", command.UserId);
                        break;
                    case InsufficientFundsError:
                        unitOfWork.InboxMessages.MarkAsFailed(message);
                        logger.LogError("Insufficient funds for order {OrderId} by {UserId}", command.OrderId, command.UserId);
                        break;
                    default:
                        unitOfWork.InboxMessages.MarkAsFailed(message);
                        logger.LogError("Unknown error for order {OrderId} by {UserId}", command.OrderId, command.UserId);
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