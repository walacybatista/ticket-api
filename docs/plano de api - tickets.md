# Plano de Implementação - API de Tickets

## 1. Caso de Uso Principal

API de suporte para criação, listagem e manipulação de tickets.

- Tickets vinculados a uma empresa
- Cada ticket tem um usuário responsável
- **Futuro**: worker consome tickets via fila, atualiza status

## 2. Entidades

### Ticket
- ID (GUID)
- Título
- Descrição
- Status (Aberto, Em Andamento, Fechado)
- Data de Criação
- ID da Empresa (FK)
- ID do Usuário (FK)

### Empresa
- ID (GUID)
- Nome
- CNPJ
- Endereço
- Data de Criação

### Usuário
- ID (GUID)
- Nome
- Email
- EmpresaID (FK) - Relação com a Empresa
- Data de Criação

### Comentário (Opcional, para futuras interações)
- ID (GUID)
- TicketID (FK)
- Texto
- Data de Criação
- ID do Usuário (FK) - Quem comentou

## 3. Stack e Ferramentas

- **Linguagem**: .NET 9 (ou .NET 8 se preferir)
- **Banco Relacional**: PostgreSQL
- **Banco Não Relacional**: MongoDB
- **Docker Compose**: para orquestrar containers do app, Postgre e Mongo
- **CQRS**: separação de comandos e queries
- **DDD**: organização em camadas (Domínio, Aplicação, Infraestrutura)
- **RabbitMQ**: fila para consumo assíncrono (futuro)
- **Redis**: cache (futuro)

## 4. Passos do Projeto

### 4.1. Configuração do Ambiente (ajuda com IA - Entendendo passo a passo (aprovando))

- [ ] Crie um diretório base do projeto
- [ ] Configure o Dockerfile para .NET
- [ ] Configure o docker-compose.yml para subir o app, Postgre e Mongo
- [ ] Verifique o ambiente com `docker-compose up`

### 4.2. Modelagem das Entidades (ajuda com IA - Entendendo passo a passo (aprovando))

- [ ] Defina as entidades no domínio (Ticket, Empresa, Usuário, Comentário)
- [ ] Crie os repositórios para cada entidade
- [ ] Implemente as regras de domínio (validações de status, integridade)

### 4.3. Implementação do CRUD (1 exemplo com IA, depois replicar para os demais (usar ia para duvidas))

Crie endpoints básicos:
- [ ] `POST`: criar ticket
- [ ] `GET`: listar tickets (filtros por status, empresa, etc.)
- [ ] `PUT`: atualizar ticket (status, descrição)
- [ ] `DELETE`: deletar ticket
- [ ] Implemente DTOs (Data Transfer Objects) para entrada/saída

### 4.4. CQRS (1 exemplo de Query e Command com IA, depois replicar para os demais (usar ia para duvidas))

- [ ] Separe os comandos (criar, atualizar, deletar) das queries (listar)
- [ ] Crie um comando para cada operação de escrita e seu handler
- [ ] Crie queries para listagem, usando uma camada de leitura otimizada

### 4.5. DDD (ajuda com IA - Entendendo passo a passo (aprovando))

Organize a estrutura em pastas:
- Domínio (Entidades, Regras)
- Aplicação (Comandos, Handlers, Serviços de Aplicação)
- Infraestrutura (Repositórios, Contexto de Banco)

### 4.6. Conexão com Bancos (ajuda com IA - Entendendo passo a passo (aprovando))

- [ ] Implemente o contexto do Postgre (Entity Framework ou Dapper)
- [ ] Implemente a conexão com o Mongo (usando MongoDB.Driver)
- [ ] Teste a inserção, busca e atualização em ambos os bancos

### 4.7. Testes Unitários e de Integração (ajuda com IA - Entendendo passo a passo (aprovando))

- [ ] Escreva testes unitários para serviços e handlers
- [ ] Escreva testes de integração para as rotas de CRUD
- [ ] Teste a comunicação com os dois bancos

### 4.8. Futuro: RabbitMQ e Redis (ajuda com IA - Entendendo passo a passo de forma detalhada e didatica (aprovando))

- [ ] Adicione o RabbitMQ e configure uma fila para processar os tickets
- [ ] Adicione Redis para cache