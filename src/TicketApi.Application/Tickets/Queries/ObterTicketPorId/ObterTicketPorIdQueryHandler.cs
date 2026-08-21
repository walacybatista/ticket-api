using MediatR;
using TicketApi.Domain.Repositories;

namespace TicketApi.Application.Tickets.Queries.ObterTicketPorId;

public class ObterTicketPorIdQueryHandler : IRequestHandler<ObterTicketPorIdQuery, TicketResponse>
{
    private readonly ITicketRepository _ticketRepository;

    public ObterTicketPorIdQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<TicketResponse> Handle(ObterTicketPorIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.ObterPorIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Ticket {request.Id} não encontrado.");

        return new TicketResponse(
            ticket.Id,
            ticket.Titulo,
            ticket.Descricao,
            ticket.Status.ToString(),
            ticket.DataCriacao,
            ticket.EmpresaId,
            ticket.UsuarioId);
    }
}
