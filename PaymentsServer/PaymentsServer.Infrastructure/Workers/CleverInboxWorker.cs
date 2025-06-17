using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentsServer.Application.Commands;
using PaymentsServer.Application.DTOs;
using PaymentsServer.Application.Queries;
using PaymentsServer.Domain.Entities;
using PaymentsServer.Domain.Interfaces;
using PaymentsServer.Domain.ValueObjects;

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
                    logger.LogError("Wrong content.");
                    continue;
                }

                var result = await mediator.Send(command, ct);
                
                unitOfWork.InboxMessages.MarkAsSuccess(message);
                
                var isSuccess = true;
                
                switch (result.Value)
                {
                    case Success:
                        logger.LogInformation("Success {OrderId}", command.OrderId);
                        break;
                    case AccountNotFoundError:
                        isSuccess = false;
                        logger.LogError("Account {UserId} not found", command.UserId);
                        break;
                    case InsufficientFundsError:
                        isSuccess = false;
                        logger.LogError("Insufficient funds for order {OrderId} by {UserId}", command.OrderId, command.UserId);
                        break;
                    default:
                        isSuccess = false;
                        logger.LogError("Unknown error for order {OrderId} by {UserId}", command.OrderId, command.UserId);
                        break;
                }
                
                var outboxContent = new OutboxMessageDto
                {
                    OrderId = command.OrderId,
                    IsSuccess = isSuccess
                };
                
                var outboxCommand = new AddOutboxMessageCommand(Type: "close-order-topic", Content: JsonSerializer.Serialize(outboxContent));
                await mediator.Send(outboxCommand, ct);
                
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