namespace TicketApi.Api.Contracts.Tickets;

public record CriarTicketRequest(
    string Titulo,
    string Descricao,
    Guid EmpresaId,
    Guid UsuarioId);