using MediatR;

namespace TicketApi.Application.Tickets.Commands.AtualizarTicket;

public record AtualizarTicketCommand(
    Guid Id,
    string Descricao
) : IRequest;
