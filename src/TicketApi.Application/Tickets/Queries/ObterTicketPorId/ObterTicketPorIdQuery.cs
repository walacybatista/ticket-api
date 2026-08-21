using MediatR;

namespace TicketApi.Application.Tickets.Queries.ObterTicketPorId;

public record ObterTicketPorIdQuery(Guid Id) : IRequest<TicketResponse>;
