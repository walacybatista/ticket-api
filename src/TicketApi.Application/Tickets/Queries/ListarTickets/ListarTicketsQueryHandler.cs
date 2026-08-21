using MediatR;
using TicketApi.Domain.Repositories;

namespace TicketApi.Application.Tickets.Queries.ListarTickets;

public class ListarTicketsQueryHandler : IRequestHandler<ListarTicketsQuery, IEnumerable<TicketResponse>>
{
    private readonly ITicketRepository _ticketRepository;

    public ListarTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<IEnumerable<TicketResponse>> Handle(ListarTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _ticketRepository.ListarAsync(request.Status, request.EmpresaId);

        return tickets.Select(t => new TicketResponse(
            t.Id,
            t.Titulo,
            t.Descricao,
            t.Status.ToString(),
            t.DataCriacao,
            t.EmpresaId,
            t.UsuarioId));
    }
}