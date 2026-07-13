using MediatR;

namespace TicketApi.Application.Tickets.Commands.IniciarAtendimentoTicket;

public record IniciarAtendimentoTicketCommand(Guid Id) : IRequest;
