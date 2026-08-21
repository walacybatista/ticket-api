# Etapa 4.2 — Modelagem das Entidades

> Documentação gerada a partir da sessão de desenvolvimento guiada. Registra as decisões arquiteturais, o código implementado e os motivos por trás de cada escolha, para servir de referência futura sobre o que foi feito e por quê.

**Status:** ✅ Concluída
**Repositório:** https://github.com/walacybatista/ticket-api.git

---

## 1. Objetivo da Etapa

Criar a estrutura de projetos seguindo Clean Architecture e modelar as entidades de domínio (`Ticket`, `Empresa`, `Usuario`, `Comentario`) com regras de negócio embutidas (Rich Domain Model / DDD), sem nenhuma dependência de infraestrutura (EF Core, MongoDB, etc.) na camada de Domínio.

---

## 2. Parte 1 — Estrutura de Projetos (Clean Architecture)

### 2.1. Camadas criadas

| Camada | Responsabilidade | Depende de |
|---|---|---|
| **Domain** | Entidades, regras de negócio puras, interfaces de repositório | Nada |
| **Application** | Casos de uso (Commands/Queries — CQRS), orquestração via MediatR | Domain |
| **Infrastructure** | Implementação dos repositórios, DbContext EF Core, driver MongoDB | Domain |
| **Api** (já existia) | Controllers, Program.cs, Swagger, DI container | Application e Infrastructure |

### 2.2. Regra de dependência

As setas de dependência sempre apontam para dentro, em direção ao `Domain`:

```
Api  →  Infrastructure  →  Domain
 └────────→  Application  →  Domain
```

O `Domain` nunca sabe que `Infrastructure` existe. `Infrastructure` é que implementa as interfaces que o `Domain` define.

### 2.3. Comandos utilizados para criar a estrutura

```bash
# Criar os projetos
dotnet new classlib -n TicketApi.Domain -o src/TicketApi.Domain
dotnet new classlib -n TicketApi.Application -o src/TicketApi.Application
dotnet new classlib -n TicketApi.Infrastructure -o src/TicketApi.Infrastructure

# Adicionar à solução
dotnet sln TicketApi.slnx add src/TicketApi.Domain/TicketApi.Domain.csproj
dotnet sln TicketApi.slnx add src/TicketApi.Application/TicketApi.Application.csproj
dotnet sln TicketApi.slnx add src/TicketApi.Infrastructure/TicketApi.Infrastructure.csproj

# Configurar referências (regra de dependência)
dotnet add src/TicketApi.Application reference src/TicketApi.Domain
dotnet add src/TicketApi.Infrastructure reference src/TicketApi.Domain
dotnet add src/TicketApi.Api reference src/TicketApi.Application
dotnet add src/TicketApi.Api reference src/TicketApi.Infrastructure
```

### 2.4. Por que essa abordagem (vs. N-Layer tradicional)

| Abordagem | Vantagem | Desvantagem |
|---|---|---|
| **N-Layer tradicional** (Controllers → Services → Repositories) | Simples, menos "cerimônia" | Entidade acoplada ao ORM; difícil testar regra de negócio isolada |
| **Clean Architecture** (escolhida) | Domain testável isoladamente; trocar banco/framework não exige tocar regra de negócio | Mais projetos, mais indireção (interfaces) |

**Quando usar Clean Architecture:** projetos com regra de negócio não trivial, que vão crescer, que exigem testabilidade forte (caso deste projeto — CQRS, múltiplos bancos, fila futura).

**Quando não usar:** protótipos descartáveis ou CRUDs muito simples sem regra de negócio real.

### 2.5. Nota sobre erros de cache do editor

Durante o setup, o VS Code (via extensão C#/OmniSharp) apresentou erros falsos (`CS1061` em `UseSwagger()`/`AddSwaggerGen()`) mesmo com `dotnet build` compilando com sucesso. Causa: o *language server* do editor mantém um cache separado do compilador real e não percebe imediatamente mudanças em `.csproj`/referências de projeto.

**Regra prática fixada:** sempre que o editor discordar do terminal, confiar no terminal — `dotnet build` é a fonte da verdade.

**Solução aplicada:** limpar `bin/` e `obj/` de todos os projetos e reiniciar o VS Code:
```bash
find . -type d -name "bin" -exec rm -rf {} +
find . -type d -name "obj" -exec rm -rf {} +
dotnet restore
dotnet build
```

---

## 3. Parte 2 — Modelagem das Entidades

### 3.1. Rich Domain Model vs Anemic Domain Model

Optou-se por **Rich Domain Model**: propriedades com `private set`, alteradas apenas através de métodos de negócio (ex: `Ticket.Fechar()`), garantindo que a entidade nunca fique em um estado inválido. Isso é o oposto do **Anemic Domain Model**, onde propriedades têm `get`/`set` públicos e a validação "vaza" para Services ou Controllers.

**Motivo da escolha:** o plano do projeto já prevê DDD explicitamente, e Rich Domain Model garante que a regra de negócio more em um único lugar — a própria entidade.

### 3.2. Classe base `Entity`

Centraliza `Id` (GUID gerado automaticamente) e `DataCriacao`, compartilhados por todas as entidades.

```csharp
namespace TicketApi.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; private set; }
    public DateTime DataCriacao { get; private set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        DataCriacao = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
```

### 3.3. Enum `StatusTicket`

```csharp
namespace TicketApi.Domain.Enums;

public enum StatusTicket
{
    Aberto = 1,
    EmAndamento = 2,
    Fechado = 3
}
```

### 3.4. Entidade `Empresa`

```csharp
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
```

### 3.5. Entidade `Usuario`

```csharp
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
```

### 3.6. Entidade `Ticket`

```csharp
using TicketApi.Domain.Common;
using TicketApi.Domain.Enums;

namespace TicketApi.Domain.Entities;

public class Ticket : Entity
{
    public string Titulo { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public StatusTicket Status { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid UsuarioId { get; private set; }

    // Exigido pelo EF Core para materializar a entidade via reflection ao ler do banco.
    // Não deve ser usado pelo código de negócio — use o construtor público.
    protected Ticket() { }

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
}
```

### 3.7. Entidade `Comentario`

Decisão de negócio: **comentário é editável** (via `EditarTexto()`), assumindo o trade-off de perder rastreabilidade/auditoria em favor de simplicidade nesta fase do projeto. Registrado como ponto de evolução futura (ver seção 5).

```csharp
using TicketApi.Domain.Common;

namespace TicketApi.Domain.Entities;

public class Comentario : Entity
{
    public Guid TicketId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Texto { get; private set; } = null!;

    // Exigido pelo EF Core para materializar a entidade via reflection ao ler do banco.
    // Não deve ser usado pelo código de negócio — use o construtor público.
    protected Comentario() { }

    public Comentario(Guid ticketId, Guid usuarioId, string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("Texto do comentário é obrigatório.", nameof(texto));

        if (ticketId == Guid.Empty)
            throw new ArgumentException("Ticket é obrigatório.", nameof(ticketId));

        if (usuarioId == Guid.Empty)
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
```

### 3.8. Interface de Repositório

```csharp
using TicketApi.Domain.Entities;

namespace TicketApi.Domain.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Ticket>> ListarAsync();
    Task AdicionarAsync(Ticket ticket);
    Task AtualizarAsync(Ticket ticket);
}
```

---

## 4. Decisões técnicas e motivos (resumo)

| Decisão | Motivo |
|---|---|
| `private set` em todas as propriedades | Impede alteração de estado fora dos métodos de negócio da entidade |
| Construtor público com validação (`ArgumentException`) | Impede a existência de um objeto em estado inválido |
| Construtor `protected` sem parâmetros e **vazio** | Exigido pelo EF Core para materializar objetos via reflection ao ler do banco; marcado `protected` para não ser usado por código de negócio. Mantido sem corpo — os valores são preenchidos ou pelo EF (na leitura) ou pelo construtor público (na criação) |
| `null!` (null-forgiving operator) **na declaração de cada propriedade `string`** (ex.: `public string Titulo { get; private set; } = null!;`) | Resolve o warning `CS8618` sem enfraquecer o `private set` nem tornar as propriedades anuláveis (`string?`), o que "vazaria" a preocupação técnica do EF para o Domain. Fica **na declaração** (e não no corpo do construtor vazio) para que o marcador técnico viva junto da propriedade a que se refere e o construtor `protected` permaneça vazio e autoexplicativo |
| `Guid`/`enum` não precisam de `null!` | São *value types* — nunca são `null`; o valor padrão de `Guid` não inicializado é `Guid.Empty`, por isso a validação `== Guid.Empty` é feita manualmente nos construtores |
| `nameof(parametro)` sempre correspondente ao parâmetro validado no `if` | Garante que a exception aponte para o campo real que falhou, essencial para debugging correto em produção |
| `TicketApi.Domain` sem nenhum pacote NuGet externo | Regra de negócio não pode depender de detalhe técnico (banco, ORM, framework web); permite trocar tecnologia de persistência sem alterar o Domain |
| Métodos de negócio nomeados por intenção (`Fechar()`, `IniciarAtendimento()`, `EditarTexto()`) em vez de setters públicos | Princípio "Tell, Don't Ask" — a entidade decide se a transição de estado é válida, em vez de expor o estado para decisão externa |

---

## 5. Erros comuns identificados durante a revisão (para não repetir)

1. **Validar a propriedade da classe em vez do parâmetro do construtor** — ex.: escrever `if (TicketId == Guid.Empty)` dentro do construtor antes de `TicketId` receber o valor de `ticketId`. Como a propriedade ainda não foi atribuída, ela sempre vale o padrão do tipo (`Guid.Empty`), fazendo a validação disparar sempre, mesmo com dados válidos.
2. **Aplicar `null!` em tipos que não são `string`** — ex.: `Status = null!` em uma propriedade `enum` (`StatusTicket`). Enums são *value types* e nunca precisam disso; o compilador nunca gera warning `CS8618` para eles.
3. **Reaproveitar a mesma mensagem de erro / `nameof()` para validações diferentes** — mensagens de exception devem refletir exatamente qual campo falhou, senão o debug fica enganoso.
4. **Confundir `Guid` com um "incrementador"** — `Guid` é um identificador gerado aleatoriamente (sem relação sequencial), diferente de colunas auto-incrementais (`SERIAL`/`IDENTITY`) do banco de dados.

---

## 6. Pontos de evolução futura (registrados, não implementados ainda)

- **Auditoria/histórico de edição de comentários**: hoje `EditarTexto()` sobrescreve o texto sem deixar rastro. Caso o negócio exija rastreabilidade (ex: disputas sobre o que foi prometido a um cliente), considerar manter o método, mas adicionar um histórico de versões separado.
- **Healthchecks no Docker Compose**, **arquivo `.env`** e **usuário não-root no container** seguem pendentes conforme já documentado em `docker-configuration.md`.

---

## 7. Próxima etapa

**Etapa 4.3 — Implementação do CRUD**, aplicando CQRS: Commands e Queries via MediatR (em vez de um Controller CRUD tradicional), e DTOs de entrada/saída. Abordagem combinada: um fluxo completo (ex: criação de Ticket) guiado camada por camada, e os demais fluxos (listar, atualizar, deletar) implementados de forma independente para revisão.
