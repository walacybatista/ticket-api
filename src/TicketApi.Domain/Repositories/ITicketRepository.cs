using TicketApi.Domain.Entities;
using TicketApi.Domain.Enums;

namespace TicketApi.Domain.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Ticket>> ListarAsync(StatusTicket? status = null, Guid? empresaId = null);
    Task AdicionarAsync(Ticket ticket);
    Task AtualizarAsync(Ticket ticket);
    Task RemoverAsync(Guid id);
}
