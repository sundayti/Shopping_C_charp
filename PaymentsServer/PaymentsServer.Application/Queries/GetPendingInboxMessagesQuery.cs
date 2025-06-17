
using MediatR;
using PaymentsServer.Domain.Entities;
using PaymentsServer.Domain.Interfaces;


namespace PaymentsServer.Application.Queries;

public record GetPendingInboxMessagesQuery(int BatchSize) : IRequest<List<InboxMessage>>;

public class GetPendingInboxMessagesQueryHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<GetPendingInboxMessagesQuery, List<InboxMessage>>
{
    public async Task<List<InboxMessage>> Handle(GetPendingInboxMessagesQuery request, CancellationToken ct)
    {
        return await unitOfWork.InboxMessages.GetReceivedAsync(request.BatchSize, ct);
    }
}
