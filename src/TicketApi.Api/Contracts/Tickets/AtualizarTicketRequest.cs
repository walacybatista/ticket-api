using TicketApi.Domain.Enums;

namespace TicketApi.Api.Contracts.Tickets;

public record AtualizarTicketRequest(
    string Descricao,
    StatusTicket Status);
