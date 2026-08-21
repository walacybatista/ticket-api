using TicketApi.Domain.Entities;

namespace TicketApi.Domain.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Ticket>> ListarAsync();
    Task AdicionarAsync(Ticket ticket);
    Task AtualizarAsync(Ticket ticket);
}