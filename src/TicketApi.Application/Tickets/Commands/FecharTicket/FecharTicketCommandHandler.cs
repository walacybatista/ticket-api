using MediatR;
using TicketApi.Domain.Repositories;

namespace TicketApi.Application.Tickets.Commands.FecharTicket;

public class FecharTicketCommandHandler : IRequestHandler<FecharTicketCommand>
{
    private readonly ITicketRepository _ticketRepository;

    public FecharTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task Handle(FecharTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.ObterPorIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Ticket {request.Id} não encontrado.");

        ticket.Fechar();

        await _ticketRepository.AtualizarAsync(ticket);
    }
}
