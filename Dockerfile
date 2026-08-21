# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia a solution e TODOS os .csproj primeiro, para aproveitar o cache de
# camadas do Docker: o "dotnet restore" só re-executa quando um .csproj muda,
# e não a cada alteração de código.
COPY ["TicketApi.slnx", "."]
COPY ["src/TicketApi.Domain/TicketApi.Domain.csproj", "src/TicketApi.Domain/"]
COPY ["src/TicketApi.Application/TicketApi.Application.csproj", "src/TicketApi.Application/"]
COPY ["src/TicketApi.Infrastructure/TicketApi.Infrastructure.csproj", "src/TicketApi.Infrastructure/"]
COPY ["src/TicketApi.Api/TicketApi.Api.csproj", "src/TicketApi.Api/"]

RUN dotnet restore "src/TicketApi.Api/TicketApi.Api.csproj"

# Agora copia o restante do código-fonte e publica
COPY . .

RUN dotnet publish "src/TicketApi.Api/TicketApi.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Roda como usuário não-root (usuário "app", UID 1654, já presente na imagem)
USER app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TicketApi.Api.dll"]
