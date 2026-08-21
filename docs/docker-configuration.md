# Docker Configuration

## 1. Visão geral

Este projeto (`ticket-api`) utiliza Docker para orquestrar três serviços que compõem o ambiente de desenvolvimento: a API em .NET 9, um banco relacional PostgreSQL e um banco não relacional MongoDB. Antes da containerização, o objetivo era eliminar divergências entre o ambiente local e o ambiente de execução — o clássico problema de "funciona na minha máquina" — garantindo que qualquer pessoa que clone o repositório consiga rodar exatamente o mesmo conjunto de versões de banco de dados e runtime, sem precisar instalar Postgres ou Mongo manualmente na máquina.

Docker se encaixa na arquitetura do projeto como camada de infraestrutura de execução: ele não faz parte da lógica de negócio (Domain/Application), mas define como a API (camada mais externa, Infrastructure/Api) e seus bancos de dados são empacotados e comunicam entre si em rede.

### Sistema operacional utilizado

- **Windows** (máquina host)
- **WSL 2** (Windows Subsystem for Linux, versão 2)
- **Docker Desktop** para Windows, com integração WSL2 habilitada

### Integração Windows + WSL + Docker

O Docker Desktop roda o Docker Engine em uma VM leve gerenciada pelo próprio Docker Desktop, e expõe os comandos `docker` e `docker-compose` para dentro de distros WSL específicas através de um recurso chamado **WSL Integration**. Isso significa que o Engine do Docker **não é instalado nativamente dentro da distro Linux** — ele é "emprestado" pelo Docker Desktop. Por isso, comandos como `docker` e `docker-compose` só funcionam dentro de uma distro WSL se essa distro estiver marcada como habilitada nas configurações do Docker Desktop.

---

## 2. Ambiente configurado

### Sistema operacional

- **Windows** (com WSL 2 habilitado)
- **Distribuição Linux utilizada**: Ubuntu 24.04 LTS (Noble Numbat)

> Nota histórica deste projeto: inicialmente havia uma distro WSL diferente (chamada apenas `Ubuntu`) configurada como padrão, que continha o .NET 10 SDK instalado via `apt`. Durante a configuração do ambiente, a distro padrão foi trocada para `Ubuntu-24.04` via `wsl --set-default Ubuntu-24.04`, que é uma distro limpa, sem o .NET 10 previamente instalado.

### Docker

- **Docker version**: 29.6.1, build 8900f1d
- **Docker Compose version**: v5.2.0
- **Docker Desktop**: utilizado como motor de execução (Docker Engine gerenciado pelo Docker Desktop, não instalado nativamente dentro do WSL)

### Recursos utilizados

| Recurso | Utilizado neste projeto? |
|---|---|
| Docker Engine | Sim (via Docker Desktop) |
| Docker CLI | Sim |
| Docker Compose | Sim (`docker-compose.yml` na raiz do projeto) |
| WSL Integration | Sim (necessário habilitar manualmente por distro) |
| Containers Linux | Sim (todas as imagens usadas são baseadas em Linux: `mcr.microsoft.com/dotnet/sdk`, `mcr.microsoft.com/dotnet/aspnet`, `postgres`, `mongo`) |
| Docker Swarm | Não configurado atualmente |
| Kubernetes | Não configurado atualmente |

---

## 3. Configuração do WSL + Docker

### Como o WSL foi configurado

O projeto foi desenvolvido dentro da distro **Ubuntu-24.04**, acessada via `wsl` a partir do PowerShell. O caminho do projeto no Windows (`C:\Users\walacy\Documents\projetos\ticket-api`) é acessado de dentro do WSL através do mount automático em `/mnt/c/Users/Walacy/Documents/Projetos/ticket-api`.

Comando usado para verificar as distros disponíveis e qual estava ativa:

```bash
wsl -l -v
```

Resultado observado neste projeto:
```
  NAME              STATE           VERSION
* Ubuntu            Running         2
  docker-desktop    Stopped         2
  Ubuntu-24.04      Running         2
```

Comando usado para trocar a distro padrão:
```bash
wsl --set-default Ubuntu-24.04
wsl --shutdown
```

### Por que não é necessário instalar Docker Engine dentro do Ubuntu

O Docker Desktop já provisiona o Engine e expõe os binários `docker` e `docker-compose` para dentro da distro WSL habilitada, via WSL Integration. Instalar o Docker Engine nativamente dentro do Ubuntu (via `apt install docker.io` ou repositório oficial da Docker) geraria um segundo Engine rodando em paralelo, o que não é necessário e pode causar conflito de configuração/rede. Por isso, neste projeto, **nenhum pacote Docker foi instalado via `apt` dentro do WSL** — toda a comunicação passa pelo Docker Desktop.

Comandos de verificação utilizados:
```bash
docker --version
docker-compose --version
```

Resultado obtido:
```
Docker version 29.6.1, build 8900f1d
Docker Compose version v5.2.0
```

> Não configurado atualmente: verificação via `docker version` (comando que mostra informações detalhadas de cliente e servidor) não foi executada neste projeto — apenas `docker --version`, que mostra só a versão do CLI.

---

## 4. Configuração de permissões Docker

**Não configurado / não aplicável neste projeto.**

O erro clássico de permissão em ambientes Linux nativos —
```
permission denied while trying to connect to the Docker API
unix:///var/run/docker.sock
```
— **não ocorreu neste projeto**. Esse erro normalmente aparece quando o Docker Engine é instalado nativamente em uma distro Linux e o usuário não pertence ao grupo `docker`. Como este projeto utiliza o Docker Desktop (que expõe o `docker`/`docker-compose` via WSL Integration, sem exigir configuração de socket/grupo dentro da distro), esse problema não se aplica ao fluxo atual.

Caso o time decida futuramente instalar o Docker Engine nativamente dentro do WSL (sem depender do Docker Desktop), a configuração de grupo seria necessária:
```bash
sudo usermod -aG docker $USER
```
seguida de reinício da sessão WSL e validação com:
```bash
groups
```
Mas isso é apenas uma referência para cenário futuro — **não foi realizado neste projeto**.

---

## 5. Validação da instalação

Testes realmente realizados neste projeto:

```bash
docker --version
docker-compose --version
```

Resultado:
```
Docker version 29.6.1, build 8900f1d
Docker Compose version v5.2.0
```

```bash
docker ps
```
Usado repetidamente para confirmar que os containers `ticket-api-api-1`, `ticket-api-postgres-1` e `ticket-api-mongo-1` estavam com status "Up".

> Não configurado atualmente: o teste padrão `docker run hello-world` não foi executado neste projeto. A validação de que o Docker Engine estava acessível veio diretamente do sucesso do `docker-compose up --build`.

---

## 6. Como iniciar o ambiente do projeto

### Primeiro acesso

```bash
wsl
cd /mnt/c/Users/Walacy/Documents/Projetos/ticket-api
```

> Importante: certifique-se de que a distro WSL usada (`Ubuntu-24.04`, neste projeto) está habilitada em Docker Desktop → Settings → Resources → WSL Integration → Apply & Restart. Sem isso, o comando `docker-compose` retorna `command not found` mesmo com o Docker Desktop aberto.

### Executar ambiente Docker

Comando real utilizado neste projeto (com rebuild da imagem, necessário sempre que o código, `Dockerfile` ou dependências do `.csproj` mudarem):

```bash
docker-compose up --build
```

Isso inicia três serviços definidos no `docker-compose.yml`:
- **api**: a aplicação .NET 9, construída a partir do `Dockerfile` multi-stage local
- **postgres**: banco relacional PostgreSQL 16
- **mongo**: banco não relacional MongoDB 7

Para rodar em segundo plano, sem travar o terminal:
```bash
docker-compose up -d
```

### Parar ambiente

```bash
docker-compose down
```
Para os containers e remove a rede criada pelo compose. Os volumes (dados dos bancos) **são preservados**.

Para também apagar os dados persistidos nos bancos (⚠️ destrutivo):
```bash
docker-compose down -v
```

### Visualizar containers

```bash
docker ps
```
Lista containers em execução. Usado neste projeto para confirmar que os 3 serviços estavam com status "Up" e checar o mapeamento de portas.

```bash
docker ps -a
```
Lista também containers parados (não utilizado extensivamente neste projeto até o momento, mas disponível).

### Logs

```bash
docker-compose logs api
```
Usado neste projeto especificamente para isolar logs do serviço `api` e diagnosticar se a aplicação havia subido corretamente, sem misturar com o volume de logs do Mongo e Postgres.

```bash
docker-compose logs -f
```
Acompanha os logs de todos os serviços em tempo real (modo "follow").

---

## 7. Comandos úteis do dia a dia

| Comando | Descrição |
|---|---|
| `docker ps` | Lista containers ativos |
| `docker ps -a` | Lista todos os containers, incluindo parados |
| `docker images` | Lista imagens locais |
| `docker-compose logs <serviço>` | Visualiza logs de um serviço específico |
| `docker-compose logs -f` | Acompanha logs em tempo real |
| `docker exec -it <container> bash` | Executa um shell interativo dentro do container |
| `docker-compose up --build` | Reconstrói a imagem e inicia os serviços |
| `docker-compose up -d` | Inicia os serviços em segundo plano |
| `docker-compose down` | Para e remove os containers e a rede (mantém volumes) |
| `docker-compose down -v` | Para os containers e apaga também os volumes (dados dos bancos) |

---

## 8. Estrutura Docker do projeto

### Dockerfile

Localizado na raiz do projeto. Multi-stage build com duas etapas:

```dockerfile
# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia a solution e TODOS os .csproj primeiro, para aproveitar o cache de
# camadas do Docker: o "dotnet restore" só re-executa quando um .csproj muda.
COPY ["TicketApi.slnx", "."]
COPY ["src/TicketApi.Domain/TicketApi.Domain.csproj", "src/TicketApi.Domain/"]
COPY ["src/TicketApi.Application/TicketApi.Application.csproj", "src/TicketApi.Application/"]
COPY ["src/TicketApi.Infrastructure/TicketApi.Infrastructure.csproj", "src/TicketApi.Infrastructure/"]
COPY ["src/TicketApi.Api/TicketApi.Api.csproj", "src/TicketApi.Api/"]
RUN dotnet restore "src/TicketApi.Api/TicketApi.Api.csproj"

COPY . .
RUN dotnet publish "src/TicketApi.Api/TicketApi.Api.csproj" -c Release -o /app/publish --no-restore

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
USER app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TicketApi.Api.dll"]
```

> Nota: o arquivo de solução do projeto usa a extensão `.slnx` (novo formato XML de solution introduzido em versões recentes do .NET SDK), e não a extensão clássica `.sln`. Isso foi identificado e corrigido durante a configuração inicial.

**Motivo do multi-stage**: a etapa de build usa a imagem do SDK (~1GB+, contém compiladores e ferramentas), enquanto a etapa final usa apenas a imagem de runtime ASP.NET (bem mais enxuta, ~200MB), reduzindo o tamanho da imagem final e a superfície de exposição de ferramentas desnecessárias em produção.

**Por que copiar todos os `.csproj` antes do restore**: como a solução tem quatro projetos (`Domain`, `Application`, `Infrastructure`, `Api`) e o `Api` referencia os demais, copiar apenas o `.csproj` da API antes do `dotnet restore` fazia o restore **pular** os projetos referenciados (`Skipping project ... because it was not found`). O restore real acabava acontecendo dentro do `dotnet publish`, invalidando o cache de dependências a cada mudança de código. Copiando os quatro `.csproj` primeiro, a camada de restore é reaproveitada enquanto nenhum `.csproj` mudar, e o `publish` usa `--no-restore`.

**Usuário não-root**: a etapa final usa `USER app` (usuário `app`, UID 1654, já presente na imagem `aspnet:9.0`), evitando rodar o processo da aplicação como root dentro do container.

### .dockerignore

```
**/bin/
**/obj/
**/out/
**/.vs/
**/.vscode/
**/.idea/
*.user
.git/
docs/
README.md
**/.env
**/appsettings.*.Local.json
```

Evita copiar artefatos de build locais (que podem ter sido compilados para uma plataforma diferente da do container), pastas de IDE, histórico Git, documentação e possíveis segredos locais para dentro do contexto de build da imagem Docker. Reduz o tamanho do contexto enviado ao Docker e a superfície de vazamento de credenciais.

### docker-compose.yml

```yaml
services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_HTTP_PORTS=8080
      # Dentro da rede do compose, o host do banco é o nome do serviço ("postgres"), não localhost.
      - ConnectionStrings__Default=Host=postgres;Port=5432;Database=ticket_db;Username=ticket_user;Password=ticket_pass
    depends_on:
      postgres:
        condition: service_healthy
      mongo:
        condition: service_healthy
    restart: unless-stopped

  postgres:
    image: postgres:16
    environment:
      POSTGRES_USER: ticket_user
      POSTGRES_PASSWORD: ticket_pass
      POSTGRES_DB: ticket_db
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ticket_user -d ticket_db"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 10s

  mongo:
    image: mongo:7
    ports:
      - "27017:27017"
    volumes:
      - mongo_data:/data/db
    healthcheck:
      test: ["CMD", "mongosh", "--eval", "db.adminCommand('ping')"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 10s

volumes:
  postgres_data:
  mongo_data:
```

**Healthchecks + `depends_on: condition: service_healthy`**: substituem a limitação do antigo `depends_on` simples (que só garantia que o container do banco havia *iniciado*, não que estava pronto para aceitar conexões). Agora a API só é iniciada depois que Postgres e Mongo respondem aos healthchecks (`pg_isready` e `db.adminCommand('ping')`), o que foi confirmado durante a subida: os containers `postgres` e `mongo` ficam `Healthy` antes de o container `api` iniciar.

### Serviços

| Serviço | Imagem | Descrição |
|---|---|---|
| `api` | Construída localmente via `Dockerfile` | API .NET 9 |
| `postgres` | `postgres:16` | Banco relacional |
| `mongo` | `mongo:7` | Banco não relacional |

### Containers (nomes gerados pelo compose)

- `ticket-api-api-1`
- `ticket-api-postgres-1`
- `ticket-api-mongo-1`

### Portas utilizadas

| Serviço | Porta no host | Porta no container |
|---|---|---|
| api | 5000 | 8080 |
| postgres | 5432 | 5432 |
| mongo | 27017 | 27017 |

### Volumes

- `postgres_data`: persiste os dados do PostgreSQL em `/var/lib/postgresql/data`
- `mongo_data`: persiste os dados do MongoDB em `/data/db`

### Redes

Rede padrão criada automaticamente pelo Docker Compose: `ticket-api_default`. Não configurado atualmente: nenhuma rede customizada foi definida explicitamente no `docker-compose.yml`.

### Variáveis de ambiente

| Variável | Serviço | Valor |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | api | `Development` |
| `ASPNETCORE_HTTP_PORTS` | api | `8080` |
| `ConnectionStrings__Default` | api | `Host=postgres;Port=5432;Database=ticket_db;Username=ticket_user;Password=ticket_pass` |
| `POSTGRES_USER` | postgres | `ticket_user` |
| `POSTGRES_PASSWORD` | postgres | `ticket_pass` |
| `POSTGRES_DB` | postgres | `ticket_db` |

> O `ConnectionStrings__Default` usa a convenção de configuração do .NET (o `__` vira `:` em `ConnectionStrings:Default`), e o host aponta para o serviço `postgres` da rede do compose — por isso a API conecta no banco mesmo sem `localhost`. Fora do Docker, esse valor vem do `appsettings.json` apontando para `localhost:5432`.

> Não configurado atualmente: uso de arquivo `.env` separado para essas variáveis (estão hardcoded diretamente no `docker-compose.yml`). Considerar migrar para `.env` antes de subir este projeto para produção, para não versionar credenciais no Git.

### global.json (fixação de versão do SDK, relevante para builds locais fora do Docker)

```json
{
  "sdk": {
    "version": "9.0.315"
  }
}
```

Este arquivo garante que o `dotnet` CLI, quando executado fora do container, use a versão 9.0.315 do SDK, mesmo que outras versões (como .NET 10) estejam instaladas na máquina. Isso mantém consistência entre o build local e o build feito dentro do Dockerfile (que usa `mcr.microsoft.com/dotnet/sdk:9.0`).

---

## 9. Fluxo de desenvolvimento

1. Abrir o Docker Desktop
2. Abrir o WSL (`wsl` no PowerShell), certificando-se de estar na distro correta (`Ubuntu-24.04`)
3. Entrar no diretório do projeto: `cd /mnt/c/Users/Walacy/Documents/Projetos/ticket-api`
4. Executar `docker-compose up --build` (ou `docker-compose up -d` para rodar em segundo plano)
5. Acessar `http://localhost:5000/swagger` para confirmar que a API está respondendo
6. Desenvolver e testar as mudanças
7. Acompanhar logs com `docker-compose logs -f` quando necessário depurar
8. Derrubar o ambiente ao finalizar: `docker-compose down` (ou `down -v` se quiser resetar os dados dos bancos)

---

## 10. Troubleshooting

### `docker-compose: command not found` dentro do WSL

**Causa real observada neste projeto**: a distro WSL padrão foi trocada (`Ubuntu` → `Ubuntu-24.04`), e a integração do Docker Desktop com WSL ainda estava configurada apenas para a distro antiga.

**Solução**:
1. Abrir o Docker Desktop
2. Settings → Resources → WSL Integration
3. Habilitar o toggle para a distro em uso (`Ubuntu-24.04`)
4. Apply & Restart
5. Fechar e reabrir o terminal WSL

### Erro `NETSDK1045` — versão do .NET SDK não suportada

**Causa real observada neste projeto**: o projeto `.csproj` foi gerado com `<TargetFramework>net10.0</TargetFramework>` (porque o SDK mais recente instalado na máquina era o .NET 10), mas o `Dockerfile` usava imagens `sdk:9.0`/`aspnet:9.0`.

**Solução aplicada**: instalação do .NET 9 SDK via script oficial (`dotnet-install.sh`), criação de um `global.json` fixando a versão 9.0.315, e ajuste do `TargetFramework` no `.csproj` para `net9.0`.

### Container sobe mas a aplicação não conecta ao banco

Historicamente, o `depends_on` simples no `docker-compose.yml` garantia apenas que o container do banco **iniciou**, não que ele já estava pronto para aceitar conexões. **Isto já foi resolvido**: o compose agora usa `depends_on: condition: service_healthy` com healthchecks em Postgres e Mongo, de modo que a API só sobe após os bancos estarem prontos.

### Porta já em uso (5000, 5432 ou 27017)

Outro processo ou container já está usando a porta. Alterar o mapeamento de portas no `docker-compose.yml` (ex: `"5433:5432"`) ou parar o processo conflitante.

---

## 11. Próximas configurações possíveis

- ✅ **Healthchecks** no `docker-compose.yml` para Postgres e Mongo — **implementado** (ver seções 8 e 10).
- ✅ **Segurança de containers**: rodar o container da API com usuário não-root — **implementado** via `USER app` no `Dockerfile` (ver seção 8).
- **Arquivo `.env`** para externalizar credenciais do Postgres, evitando hardcode no `docker-compose.yml`
- **Docker secrets** para gerenciamento seguro de credenciais em ambientes de produção/orquestração (Swarm/Kubernetes)
- **CI/CD**: pipeline que builda a imagem Docker automaticamente a cada push, rodando testes antes do build
- **Otimização adicional de imagem**: uso de imagens `-alpine` para reduzir ainda mais o tamanho final, se compatibilidade permitir
- **RabbitMQ e Redis**: adicionar como novos serviços no `docker-compose.yml` quando essas funcionalidades forem implementadas (previsto no plano original do projeto)
- ✅ **Conexão real da API com o PostgreSQL**: **implementado** — a API agora persiste tickets no Postgres via EF Core + Npgsql (`PostgresTicketRepository`), com a connection string injetada por variável de ambiente (`ConnectionStrings__Default`, host `postgres`) e migrations aplicadas automaticamente na subida. O antigo `InMemoryTicketRepository` foi removido.
- **Conexão com o MongoDB**: o serviço `mongo` sobe mas ainda **não é consumido** pela aplicação — falta implementar a integração com o `MongoDB.Driver` (previsto no plano original).
