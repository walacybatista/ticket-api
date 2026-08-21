namespace TicketApi.Application.Tickets.Queries;

public record TicketResponse(
    Guid Id,
    string Titulo,
    string Descricao,
    string Status,
    DateTime DataCriacao,
    Guid EmpresaId,
    Guid UsuarioId);