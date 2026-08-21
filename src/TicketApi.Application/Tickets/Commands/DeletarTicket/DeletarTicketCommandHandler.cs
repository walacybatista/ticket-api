using MediatR;
using TicketApi.Domain.Repositories;

namespace TicketApi.Application.Tickets.Commands.DeletarTicket;

public class DeletarTicketCommandHandler : IRequestHandler<DeletarTicketCommand>
{
    private readonly ITicketRepository _ticketRepository;

    public DeletarTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task Handle(DeletarTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.ObterPorIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Ticket {request.Id} não encontrado.");

        await _ticketRepository.RemoverAsync(ticket.Id);
    }
}
