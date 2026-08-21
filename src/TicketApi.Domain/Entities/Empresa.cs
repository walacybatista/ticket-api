using TicketApi.Domain.Common;

namespace TicketApi.Domain.Entities;

public class Empresa : Entity
{
    public string Nome { get; private set; } = null!;
    public string Cnpj { get; private set; } = null!;
    public string Endereco { get; private set; } = null!;

    // Exigido pelo EF Core para materializar a entidade via reflection ao ler do banco.
    // Não deve ser usado pelo código de negócio — use o construtor público.
    protected Empresa() { }

    public Empresa(string nome, string cnpj, string endereco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da empresa é obrigatório.", nameof(nome));

        if (string.IsNullOrWhiteSpace(cnpj))
            throw new ArgumentException("CNPJ é obrigatório.", nameof(cnpj));

        Nome = nome;
        Cnpj = cnpj;
        Endereco = endereco;
    }
}