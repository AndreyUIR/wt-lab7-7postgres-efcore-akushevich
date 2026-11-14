# Версия .NET: должна совпадать с TargetFramework в .csproj
ARG DOTNET_VERSION=9.0

# ---------- Стадия 1: сборка ----------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Сначала только файл проекта: слой с пакетами кешируется
COPY *.csproj ./
RUN dotnet restore

# Затем весь остальной код
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false --no-restore

# ---------- Стадия 2: запуск ----------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "wt-lab7-7postgres-efcore-akushevich.dll"]