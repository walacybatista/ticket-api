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
   docker-compose up
   ```

3. A API estará disponível em `http://localhost:<porta>` (definir porta conforme configuração do `docker-compose.yml`).

## 📡 Endpoints da API

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/tickets` | Cria um novo ticket |
| `GET` | `/tickets` | Lista tickets (com filtros por status, empresa, etc.) |
| `PUT` | `/tickets/{id}` | Atualiza um ticket (status, descrição) |
| `DELETE` | `/tickets/{id}` | Remove um ticket |

> DTOs (Data Transfer Objects) são utilizados para entrada e saída de dados em todos os endpoints.

## 🗺️ Roadmap

- [x] Definição do plano de implementação
- [x] Configuração do ambiente (Docker + docker-compose)
- [ ] Modelagem das entidades no domínio
- [ ] Implementação do CRUD de tickets
- [ ] Separação em CQRS (Comandos e Queries)
- [ ] Organização da estrutura DDD (Domínio, Aplicação, Infraestrutura)
- [ ] Conexão com PostgreSQL (EF Core ou Dapper) e MongoDB (MongoDB.Driver)
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