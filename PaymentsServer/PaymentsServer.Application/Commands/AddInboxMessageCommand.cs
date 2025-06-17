using MediatR;
using PaymentsServer.Domain.Entities;
using PaymentsServer.Domain.Interfaces;
using PaymentsServer.Domain.ValueObjects;
namespace PaymentsServer.Application.Commands;

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
