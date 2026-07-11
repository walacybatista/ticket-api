using TicketApi.Domain.Common;

namespace TicketApi.Domain.Entities;

public class Usuario : Entity
{
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public Guid EmpresaId { get; private set; }

    protected Usuario()
    {
        // resolve waring para DDD
        Nome = null!;
        Email = null!;
    }

    public Usuario(string nome, string email, Guid empresaId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do usuário é obrigatório.", nameof(nome));

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Email inválido.", nameof(email));

        if (empresaId == Guid.Empty)
            throw new ArgumentException("Usuário precisa estar vinculado a uma empresa.", nameof(empresaId));

        Nome = nome;
        Email = email;
        EmpresaId = empresaId;
    }
}