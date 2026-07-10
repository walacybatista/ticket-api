# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["TicketApi.slnx", "."]
COPY ["src/TicketApi.Api/TicketApi.Api.csproj", "src/TicketApi.Api/"]

RUN dotnet restore "src/TicketApi.Api/TicketApi.Api.csproj"

COPY . .

RUN dotnet publish "src/TicketApi.Api/TicketApi.Api.csproj" -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TicketApi.Api.dll"]