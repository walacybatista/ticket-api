using TicketApi.Domain.Common;

namespace TicketApi.Domain.Entities;

public class Usuario : Entity
{
    public string Nome { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public Guid EmpresaId { get; private set; }

    // Exigido pelo EF Core para materializar a entidade via reflection ao ler do banco.
    // Não deve ser usado pelo código de negócio — use o construtor público.
    protected Usuario() { }

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