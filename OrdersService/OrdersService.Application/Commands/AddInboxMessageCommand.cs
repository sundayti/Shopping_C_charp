using MediatR;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;
using OrdersService.Domain.ValueObjects;

namespace OrdersService.Application.Commands;

public record AddInboxMessageCommand(Guid Id, string Type, string Content) : IRequest;

public class AddInboxMessageCommandHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<AddInboxMessageCommand>
{
    public async Task Handle(AddInboxMessageCommand request, CancellationToken ct)
    {
        var inboxMessage = new InboxMessage
        {
            Id = request.Id,
            Type = request.Type,
            Content = request.Content,
            Status = InboxMessageStatus.Received
        };

        await unitOfWork.InboxMessages.AddAsync(inboxMessage, ct);
        
        await unitOfWork.SaveChangesAsync(ct);
    }
}