using MediatR;

namespace TicketApi.Application.Tickets.Commands.CriarTicket;

public record CriarTicketCommand(
    string Titulo,
    string Descricao,
    Guid EmpresaId,
    Guid UsuarioId
) : IRequest<Guid>;