using TicketApi.Domain.Common;

namespace TicketApi.Domain.Entities;

public class Empresa : Entity
{
    public string Nome { get; private set; }
    public string Cnpj { get; private set; }
    public string Endereco { get; private set; }

    protected Empresa()
    {
        // Resolve warning para uso do DDD
        Nome = null!;
        Cnpj = null!;
        Endereco = null!;
    }

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