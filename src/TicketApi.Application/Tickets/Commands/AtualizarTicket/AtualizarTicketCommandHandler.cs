using MediatR;
using TicketApi.Domain.Repositories;

namespace TicketApi.Application.Tickets.Commands.AtualizarTicket;

public class AtualizarTicketCommandHandler : IRequestHandler<AtualizarTicketCommand>
{
    private readonly ITicketRepository _ticketRepository;

    public AtualizarTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task Handle(AtualizarTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.ObterPorIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Ticket {request.Id} não encontrado.");

        ticket.AtualizarDescricao(request.Descricao);
        ticket.AlterarStatus(request.Status);

        await _ticketRepository.AtualizarAsync(ticket);
    }
}
