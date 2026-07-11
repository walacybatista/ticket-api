using MediatR;
using TicketApi.Domain.Enums;

namespace TicketApi.Application.Tickets.Commands.AtualizarTicket;

public record AtualizarTicketCommand(
    Guid Id,
    string Descricao,
    StatusTicket Status
) : IRequest;
