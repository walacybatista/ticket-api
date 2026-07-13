using MediatR;
using TicketApi.Domain.Repositories;

namespace TicketApi.Application.Tickets.Commands.IniciarAtendimentoTicket;

public class IniciarAtendimentoTicketCommandHandler : IRequestHandler<IniciarAtendimentoTicketCommand>
{
    private readonly ITicketRepository _ticketRepository;

    public IniciarAtendimentoTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task Handle(IniciarAtendimentoTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.ObterPorIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Ticket {request.Id} não encontrado.");

        ticket.IniciarAtendimento();

        await _ticketRepository.AtualizarAsync(ticket);
    }
}
