using MediatR;
using PaymentsServer.Domain.Entities;

namespace PaymentsServer.Application.Queries;

public record GetPendingInboxMessagesQuery(int BatchSize) : IRequest<List<InboxMessage>>;
