# ==========================================
# ETAPA 1 — COMPILACIÓN
# ==========================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY backend/src/ ./backend/src/

RUN dotnet restore \
    backend/src/Inventario.Presentation/Inventario.Presentation.csproj

RUN dotnet publish \
    backend/src/Inventario.Presentation/Inventario.Presentation.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ==========================================
# ETAPA 2 — EJECUCIÓN
# ==========================================

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app/publish .

USER $APP_UID

EXPOSE 5000

ENTRYPOINT ["dotnet", "Inventario.Presentation.dll"]