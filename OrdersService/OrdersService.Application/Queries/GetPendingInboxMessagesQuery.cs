using MediatR;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Queries;

public record GetPendingInboxMessagesQuery(int BatchSize) : IRequest<List<InboxMessage>>;

public class GetPendingInboxMessagesQueryHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<GetPendingInboxMessagesQuery, List<InboxMessage>>
{
    public async Task<List<InboxMessage>> Handle(GetPendingInboxMessagesQuery request, CancellationToken ct)
    {
        return await unitOfWork.InboxMessages.GetReceivedAsync(request.BatchSize, ct);
    }
}