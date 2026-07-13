# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

COPY Docker-Mcp/Docker-Mcp.csproj Docker-Mcp/
RUN dotnet restore Docker-Mcp/Docker-Mcp.csproj

COPY . .

RUN dotnet publish Docker-Mcp/Docker-Mcp.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/runtime:10.0

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_EnableDiagnostics=0

WORKDIR /app

COPY --from=builder /app/publish .

ENTRYPOINT ["dotnet", "Docker-Mcp.dll"]
