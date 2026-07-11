# Etapa 4.3 — Implementação do CRUD de Tickets

> Documentação gerada a partir da sessão de desenvolvimento guiada. Registra o que foi implementado, as decisões de design pendentes de validação e os pontos de atenção identificados, para servir de referência futura sobre o que foi feito e por quê.

**Status:** ⚠️ Funcionalmente concluída, com decisões de design pendentes de confirmação
**Etapa anterior:** [`modelagem-entidades.md`](./modelagem-entidades.md) (4.2 — Modelagem das Entidades)
**Próxima etapa:** 4.4 — CQRS (já parcialmente antecipada nesta etapa) / 4.6 — Conexão com Bancos

---

## 1. Objetivo da Etapa

Implementar o CRUD completo de Tickets (`POST`, `GET`, `PUT`, `DELETE`) seguindo CQRS via MediatR, com DTOs de entrada/saída, tratamento de erros centralizado e validação automática — mantendo a regra de negócio na entidade de domínio (Rich Domain Model), conforme estabelecido na etapa 4.2.

---

## 2. Fluxo arquitetural implementado

```
Controller → MediatR.Send → [ValidationBehavior] → Handler → ITicketRepository → Entidade (regra de negócio)
```

Erros sobem como exceção e são traduzidos em respostas HTTP pelo `GlobalExceptionHandler`.

Esse fluxo respeita a regra de dependência da Clean Architecture (`Api → Application → Domain`) e a separação CQRS (Commands para escrita, Queries para leitura, sem regra de negócio nas Queries).

---

## 3. CRUD implementado

| Operação | Endpoint | Tipo (CQRS) | Arquivo | Status HTTP |
|---|---|---|---|---|
| Criar ticket | `POST /api/tickets` | Command | `Commands/CriarTicket/` (command + validator + handler) | `201 Created` |
| Listar tickets (com filtros) | `GET /api/tickets?status=&empresaId=` | Query | `Queries/ListarTickets/` | `200 OK` |
| Obter ticket por id | `GET /api/tickets/{id}` | Query | `Queries/ObterTicketPorId/` | `200 OK` / `404` |
| Atualizar ticket (descrição + status) | `PUT /api/tickets/{id}` | Command | `Commands/AtualizarTicket/` | `200/204` / `404` / `409` |
| Deletar ticket | `DELETE /api/tickets/{id}` | Command | `Commands/DeletarTicket/` | `204` / `404` |

### DTOs

- **Entrada**: `CriarTicketRequest`, `AtualizarTicketRequest`
- **Saída**: `TicketResponse` (DTO de leitura, compartilhado entre as Queries, com `Status` exposto como texto em vez do valor numérico do enum), `TicketCriadoResponse`

### Observação sobre o `GET /{id}`

Não estava listado explicitamente no plano original (`plano de api - tickets.md`, seção 4.3), mas foi necessário para:
- Dar coerência REST ao recurso (todo recurso listável deveria ser obtível individualmente);
- Servir de alvo correto para o `Location` header do `CreatedAtAction` no `POST /api/tickets` — antes apontava para o próprio endpoint de criação, o que não seguia o padrão REST.

---

## 4. Componentes de infraestrutura da etapa

### 4.1. `GlobalExceptionHandler`

Implementa `IExceptionHandler` (abordagem nativa do .NET 8+) para tratamento centralizado de exceções, eliminando a necessidade de `try/catch` em cada Controller. Mapeamento de exceções para HTTP:

| Exceção | Status HTTP | Cenário |
|---|---|---|
| `ValidationException` (FluentValidation) | `400 Bad Request` | Falha de validação de entrada |
| `ArgumentException` | `400 Bad Request` | Violação de invariante no construtor da entidade |
| `InvalidOperationException` | `409 Conflict` | Transição de estado inválida na entidade |
| `KeyNotFoundException` | `404 Not Found` | Recurso não encontrado |
| Qualquer outra | `500 Internal Server Error` | Erro inesperado (logado com stack trace completo) |

Respostas seguem o formato padronizado `ProblemDetails` (RFC 7807).

### 4.2. `ValidationBehavior` (pipeline do MediatR)

Intercepta toda mensagem enviada via `IMediator`/`ISender` e executa o `FluentValidation` correspondente antes do Handler ser chamado, evitando repetição de `validator.Validate()` em cada Handler.

**Bug identificado e corrigido durante esta etapa:** o `ValidationBehavior` não validava requests sem retorno (`IRequest` puro, sem tipo genérico) — isso fazia com que o `PUT` de atualização aceitasse valores de status inválidos silenciosamente, sem passar pela validação. Correção aplicada com a constraint `where TRequest : notnull`.

---

## 5. Decisões de design tomadas durante a implementação — **pendentes de confirmação**

Duas decisões de negócio foram tomadas no momento da implementação e ainda **não foram formalmente validadas** com o dono do domínio. Registradas aqui para revisão consciente antes de considerar a etapa definitivamente fechada.

### 5.1. `AtualizarTicketCommand` (PUT)

**O que foi implementado:** um único comando que atualiza descrição e status juntos, através de um novo método `Ticket.AlterarStatus()`, que inclui a regra "não é possível reabrir um ticket `Fechado`" (retorna `409 Conflict`).

**Tensão de design identificada:** a entidade `Ticket` (definida na etapa 4.2) já possuía métodos de transição de estado com regras próprias e mais restritivas:
- `IniciarAtendimento()` — só permite a transição a partir de `Aberto`
- `Fechar()` — não permite fechar um ticket já `Fechado`

O novo `AlterarStatus()` **contorna** essas regras — por exemplo, permite ir de `Aberto` direto para `Fechado`, pulando `EmAndamento`, o que os métodos originais não permitiam dessa forma. Ou seja, hoje existem **duas modelagens diferentes para a mesma transição de estado** convivendo na entidade.

**Alternativas a decidir:**

| Opção | Descrição |
|---|---|
| **A. Manter o PUT genérico** (atual) | Um único endpoint aceita qualquer novo status; simples para o cliente da API, mas enfraquece as regras de transição já definidas nos métodos de domínio |
| **B. Endpoints de transição dedicados** | `PUT /api/tickets/{id}` só atualiza descrição; transições de status via `POST /api/tickets/{id}/iniciar` e `POST /api/tickets/{id}/fechar`, reaproveitando `IniciarAtendimento()`/`Fechar()` como já modelados |

**Ação necessária:** decidir entre A e B (ou uma variação) antes de considerar este ponto fechado.

### 5.2. `DeletarTicketCommand` (DELETE)

**O que foi implementado:** delete físico — o registro é removido definitivamente do repositório.

**Alternativa não implementada:** soft delete (delete lógico) — adicionar campos como `Excluido`/`DataExclusao` na entidade, marcar como inativo em vez de remover, e ajustar as Queries para filtrar registros excluídos por padrão. Preserva histórico e permite auditoria/recuperação; tem custo de modelagem adicional na entidade e nas queries.

**Ação necessária:** confirmar se delete físico é aceitável para o domínio de tickets de suporte, ou se rastreabilidade é um requisito (o que já foi levantado como ponto de evolução futura para `Comentario` na etapa 4.2 — vale avaliar se a mesma preocupação se aplica aqui).

---

## 6. Pontos de atenção identificados (não bloqueiam a etapa, mas precisam de acompanhamento)

1. **Persistência ainda é in-memory.** `InMemoryTicketRepository` está marcado como temporário (`// TEMPORÁRIO ... etapa 4.6`). O CRUD funciona ponta a ponta, mas nenhum dado é gravado no PostgreSQL/MongoDB — os dados somem a cada reinício da aplicação. Isso é esperado nesta etapa (a conexão real com bancos é escopo da etapa 4.6), mas não deve ser confundido com "persistência pronta".
2. **Documentação desatualizada.** `modelagem-entidades.md` (seção 3.8) mostra `ITicketRepository` sem `RemoverAsync` e com `ListarAsync()` sem parâmetros de filtro. O código real já evoluiu para incluir `RemoverAsync` e `ListarAsync(status, empresaId)`. Recomenda-se atualizar aquele documento para refletir o estado atual da interface.
3. **Checklist da seção 2 do plano parcialmente impreciso.** O item "Crie os repositórios para cada entidade" está marcado como concluído, mas hoje só existe `ITicketRepository`. As entidades `Empresa`, `Usuario` e `Comentario` têm modelagem de domínio, mas nenhum repositório correspondente ainda. Como a etapa 4.3 tratou apenas do recurso Ticket, isso não bloqueia esta etapa, mas o item do checklist da seção 2 não está literalmente cumprido.
4. **Arquivos de template não removidos.** `WeatherForecastController.cs` e `WeatherForecast.cs` (gerados pelo template padrão do `dotnet new webapi`) ainda estão no projeto e devem ser removidos.

---

## 7. Decisões técnicas e motivos (resumo)

| Decisão | Motivo |
|---|---|
| `TicketResponse` definido em `Application`, não em `Api` | Evita que o Handler da Query (camada `Application`) precise depender do projeto `Api`, o que violaria a regra de dependência da Clean Architecture (`Api → Application`, nunca o inverso) |
| `Status` exposto como `string` no DTO de saída, não como o valor numérico do enum | Desacopla o cliente da API do valor interno do enum; mais legível e mais estável a reordenações futuras do enum |
| `IExceptionHandler` (nativo do .NET 8+) em vez de middleware manual | Abordagem recomendada atualmente pela Microsoft; elimina `try/catch` repetido em cada Controller |
| `KeyNotFoundException` para "não encontrado" nas Queries | Mapeada automaticamente pelo `GlobalExceptionHandler` para `404`, sem necessidade de tratamento manual no Controller |
| `IEnumerable<TicketResponse>` como tipo de retorno da listagem, não `List<TicketResponse>` | Expõe o tipo mais abstrato possível no contrato da Query — "programe para a interface, não para a implementação" |

---

## 8. Próximos passos

- [ ] Decidir e ajustar o design do `PUT` (opção A ou B, seção 5.1)
- [ ] Decidir sobre delete físico vs. lógico (seção 5.2)
- [ ] Atualizar `modelagem-entidades.md` (seção 3.8) para refletir a interface `ITicketRepository` atual
- [ ] Remover arquivos de template (`WeatherForecastController.cs`, `WeatherForecast.cs`)
- [ ] Avaliar se o item "repositórios para cada entidade" da seção 2 do plano deve ser reaberto para `Empresa`, `Usuario` e `Comentario`, ou se permanece fora do escopo até essas entidades terem casos de uso próprios
- [ ] Etapa 4.6 — substituir `InMemoryTicketRepository` por implementação real com EF Core/PostgreSQL, sem alterar Application/Domain (validação prática do Dependency Inversion Principle já aplicado desde a etapa 4.2)
