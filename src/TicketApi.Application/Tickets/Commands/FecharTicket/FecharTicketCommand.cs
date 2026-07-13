using MediatR;

namespace TicketApi.Application.Tickets.Commands.FecharTicket;

public record FecharTicketCommand(Guid Id) : IRequest;
