using MediatR;
using PaymentsServer.Domain.Entities;
using PaymentsServer.Domain.Interfaces;
using PaymentsServer.Domain.ValueObjects;

namespace PaymentsServer.Application.Commands;

public record AddOutboxMessageCommand(string Type, string Content) : IRequest;

public class AddOutboxMessageCommandHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<AddOutboxMessageCommand>
{
    public async Task Handle(AddOutboxMessageCommand request, CancellationToken ct)
    {
        var outboxMessage = new OutboxMessage
        {
            Type = request.Type,
            Content = request.Content,
            Status = OutboxMessageStatus.InProgress
        };

        await unitOfWork.OutboxMessages.AddAsync(outboxMessage, ct);
        
        await unitOfWork.SaveChangesAsync(ct);
    }
}