using MediatR;
using TicketApi.Domain.Enums;

namespace TicketApi.Application.Tickets.Queries.ListarTickets;

public record ListarTicketsQuery(
    StatusTicket? Status = null,
    Guid? EmpresaId = null
) : IRequest<IEnumerable<TicketResponse>>;
