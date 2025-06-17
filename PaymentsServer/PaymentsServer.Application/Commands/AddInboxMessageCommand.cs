using MediatR;

namespace PaymentsServer.Application.Commands;

public record AddInboxMessageCommand(Guid Id, string Type, string Content) : IRequest;
