using TicketApi.Domain.Common;

namespace TicketApi.Domain.Entities;

public class Comentario : Entity
{
    public Guid TicketId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Texto { get; private set; }

    protected Comentario()
    {
        // Resolve warning para uso do DDD
        Texto = null!;
    }

    public Comentario(Guid ticketId, Guid usuarioId, string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("Texto do comentário é obrigatório.", nameof(texto));

        if (ticketId == Guid.Empty)
            throw new ArgumentException("Ticket é obrigatório.", nameof(ticketId));

        if(usuarioId == Guid.Empty)
            throw new ArgumentException("Usuário é obrigatório.", nameof(usuarioId));

        TicketId = ticketId;
        UsuarioId = usuarioId;
        Texto = texto;
    }

    public void EditarTexto(string novoTexto)
    {
        if (string.IsNullOrWhiteSpace(novoTexto))
            throw new ArgumentException("Texto do comentário é obrigatório.", nameof(novoTexto));

        Texto = novoTexto;
    }   
}