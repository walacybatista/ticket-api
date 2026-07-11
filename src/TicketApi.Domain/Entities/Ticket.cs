using TicketApi.Domain.Common;
using TicketApi.Domain.Enums;

namespace TicketApi.Domain.Entities;

public class Ticket : Entity
{
    public string Titulo { get; private set; }
    public string Descricao { get; private set; }
    public StatusTicket Status { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid UsuarioId { get; private set; }

    protected Ticket()
    {
        // Resolve warning para uso do DDD
        Titulo = null!;
        Descricao = null!;
    }

    public Ticket(string titulo, string descricao, Guid empresaId, Guid usuarioId)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException("Título é obrigatório.", nameof(titulo));

        if (empresaId == Guid.Empty)
            throw new ArgumentException("Ticket precisa estar vinculado a uma empresa.", nameof(empresaId));

        if (usuarioId == Guid.Empty)
            throw new ArgumentException("Ticket precisa ter um usuário responsável.", nameof(usuarioId));

        Titulo = titulo;
        Descricao = descricao;
        EmpresaId = empresaId;
        UsuarioId = usuarioId;
        Status = StatusTicket.Aberto;
    }

    public void IniciarAtendimento()
    {
        if (Status != StatusTicket.Aberto)
            throw new InvalidOperationException("Só é possível iniciar atendimento de um ticket Aberto.");

        Status = StatusTicket.EmAndamento;
    }

    public void Fechar()
    {
        if (Status == StatusTicket.Fechado)
            throw new InvalidOperationException("Ticket já está fechado.");

        Status = StatusTicket.Fechado;
    }

    public void AtualizarDescricao(string descricao)
    {
        Descricao = descricao;
    }

    public void AlterarStatus(StatusTicket novoStatus)
    {
        if (Status == StatusTicket.Fechado && novoStatus != StatusTicket.Fechado)
            throw new InvalidOperationException("Não é possível reabrir um ticket fechado.");

        Status = novoStatus;
    }
}