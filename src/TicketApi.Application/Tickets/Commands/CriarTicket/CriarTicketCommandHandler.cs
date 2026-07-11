using MediatR;
using TicketApi.Domain.Entities;
using TicketApi.Domain.Repositories;

namespace TicketApi.Application.Tickets.Commands.CriarTicket;

public class CriarTicketCommandHandler : IRequestHandler<CriarTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public CriarTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(CriarTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = new Ticket(
            request.Titulo,
            request.Descricao,
            request.EmpresaId,
            request.UsuarioId);

        await _ticketRepository.AdicionarAsync(ticket);

        return ticket.Id;
    }
}