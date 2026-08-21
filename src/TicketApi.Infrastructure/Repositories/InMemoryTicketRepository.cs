using System.Collections.Concurrent;
using TicketApi.Domain.Entities;
using TicketApi.Domain.Enums;
using TicketApi.Domain.Repositories;

namespace TicketApi.Infrastructure.Repositories;

// TEMPORÁRIO: será substituído pela implementação com EF Core/PostgreSQL na etapa 4.6.
// Existe só para permitirmos testar o fluxo de CQRS de ponta a ponta agora.
public class InMemoryTicketRepository : ITicketRepository
{
    private readonly ConcurrentDictionary<Guid, Ticket> _tickets = new();

    public Task<Ticket?> ObterPorIdAsync(Guid id)
    {
        _tickets.TryGetValue(id, out var ticket);
        return Task.FromResult(ticket);
    }

    public Task<IEnumerable<Ticket>> ListarAsync(StatusTicket? status = null, Guid? empresaId = null)
    {
        var query = _tickets.Values.AsEnumerable();

        if (status is not null)
            query = query.Where(t => t.Status == status);

        if (empresaId is not null)
            query = query.Where(t => t.EmpresaId == empresaId);

        return Task.FromResult(query);
    }

    public Task AdicionarAsync(Ticket ticket)
    {
        _tickets[ticket.Id] = ticket;
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Ticket ticket)
    {
        _tickets[ticket.Id] = ticket;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid id)
    {
        _tickets.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
