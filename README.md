# API de Tickets

API de suporte para criação, listagem e gerenciamento de tickets, desenvolvida com foco em boas práticas de arquitetura (DDD e CQRS) e stack moderna .NET.

## 📋 Sobre o Projeto

O sistema permite que empresas gerenciem tickets de suporte, onde cada ticket é vinculado a uma empresa e possui um usuário responsável.

**Futuro:** um worker consumirá tickets via fila de mensageria, atualizando seus status de forma assíncrona.

## 🧩 Entidades

### Ticket
| Campo | Descrição |
|---|---|
| ID | GUID |
| Título | Título do ticket |
| Descrição | Descrição detalhada |
| Status | Aberto, Em Andamento, Fechado |
| Data de Criação | Timestamp de criação |
| ID da Empresa | FK para Empresa |
| ID do Usuário | FK para Usuário responsável |

### Empresa
| Campo | Descrição |
|---|---|
| ID | GUID |
| Nome | Nome da empresa |
| CNPJ | Documento da empresa |
| Endereço | Endereço da empresa |
| Data de Criação | Timestamp de criação |

### Usuário
| Campo | Descrição |
|---|---|
| ID | GUID |
| Nome | Nome do usuário |
| Email | E-mail do usuário |
| EmpresaID | FK para Empresa |
| Data de Criação | Timestamp de criação |

### Comentário *(opcional, para futuras interações)*
| Campo | Descrição |
|---|---|
| ID | GUID |
| TicketID | FK para Ticket |
| Texto | Conteúdo do comentário |
| Data de Criação | Timestamp de criação |
| ID do Usuário | FK para quem comentou |

## 🛠️ Stack e Ferramentas

- **Linguagem:** .NET 9 (ou .NET 8)
- **Banco Relacional:** PostgreSQL
- **Banco Não Relacional:** MongoDB
- **Orquestração:** Docker Compose (app, Postgres e Mongo)
- **Arquitetura:** DDD (Domain-Driven Design)
- **Padrão:** CQRS (separação de comandos e queries)
- **Mensageria (futuro):** RabbitMQ
- **Cache (futuro):** Redis

## 🏗️ Arquitetura

O projeto é organizado em camadas seguindo os princípios de DDD:

```
├── src/
│   ├── Domain/           # Entidades, regras de negócio e validações
│   ├── Application/      # Comandos, Queries, Handlers e serviços de aplicação
│   └── Infrastructure/   # Repositórios, contexto de banco de dados e integrações
```

## 🚀 Como Executar

### Pré-requisitos

- [Docker](https://www.docker.com/) e Docker Compose instalados
- .NET SDK 8 ou 9 (para desenvolvimento local)

### Passos

1. Clone o repositório:
   ```bash
   git clone <url-do-repositorio>
   cd <nome-do-projeto>
   ```

2. Suba os containers da aplicação, PostgreSQL e MongoDB:
   ```bash
   docker compose up -d --build
   ```
   Os healthchecks garantem que a API só inicia depois que Postgres e Mongo estão prontos. Ao subir, a API aplica automaticamente as **migrations** pendentes do EF Core, criando a tabela `tickets` no PostgreSQL.

3. A API estará disponível em `http://localhost:5000` (mapeada de `8080` no container). O Swagger fica em `http://localhost:5000/swagger`.

4. Para parar o ambiente (mantendo os dados dos bancos):
   ```bash
   docker compose down
   ```
   Use `docker compose down -v` para apagar também os volumes (dados do Postgres/Mongo).

### Banco de dados e migrations

A persistência de tickets usa **EF Core + Npgsql (PostgreSQL)**. A connection string é lida de `ConnectionStrings:Default`:

- Em Docker, é injetada via variável de ambiente no `docker-compose.yml`, apontando para o host `postgres` (nome do serviço na rede do compose).
- Em execução local (`dotnet run`, fora do Docker), vem de `appsettings.json`, apontando para `localhost:5432` — nesse caso, suba só os bancos com `docker compose up -d postgres mongo`.

Para criar novas migrations (requer a ferramenta `dotnet-ef`: `dotnet tool install --global dotnet-ef`):

```bash
dotnet ef migrations add <NomeDaMigration> \
  -p src/TicketApi.Infrastructure -s src/TicketApi.Api -o Persistence/Migrations
```

As migrations são aplicadas automaticamente na subida da API (`dbContext.Database.Migrate()` em `Program.cs`).

## 📡 Endpoints da API

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/tickets` | Cria um novo ticket |
| `GET` | `/api/tickets` | Lista tickets (filtros opcionais `status` e `empresaId`) |
| `GET` | `/api/tickets/{id}` | Obtém um ticket por id |
| `PUT` | `/api/tickets/{id}` | Atualiza um ticket (status, descrição) |
| `DELETE` | `/api/tickets/{id}` | Remove um ticket |

> DTOs (Data Transfer Objects) são utilizados para entrada e saída de dados em todos os endpoints.

## 🗺️ Roadmap

- [x] Definição do plano de implementação
- [x] Configuração do ambiente (Docker + docker-compose)
- [x] Modelagem das entidades no domínio
- [x] Implementação do CRUD de tickets
- [x] Separação em CQRS (Comandos e Queries)
- [x] Organização da estrutura DDD (Domínio, Aplicação, Infraestrutura)
- [x] Conexão com PostgreSQL (EF Core + Npgsql) — persistência real de tickets
- [ ] Conexão com MongoDB (MongoDB.Driver)
- [ ] Testes unitários e de integração
- [ ] Integração com RabbitMQ para processamento assíncrono
- [ ] Cache com Redis

## 🧪 Testes

O projeto contará com:
- Testes unitários para serviços e handlers
- Testes de integração para as rotas de CRUD
- Testes de comunicação com PostgreSQL e MongoDB

## 📦 Próximos Passos (Futuro)

- **RabbitMQ:** fila para consumo assíncrono de tickets por um worker
- **Redis:** cache para otimização de leitura

## 📄 Licença

Defina aqui a licença do projeto (ex: MIT, Apache 2.0, etc.).