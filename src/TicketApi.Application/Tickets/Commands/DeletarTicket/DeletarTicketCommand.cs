using MediatR;

namespace TicketApi.Application.Tickets.Commands.DeletarTicket;

public record DeletarTicketCommand(Guid Id) : IRequest;
