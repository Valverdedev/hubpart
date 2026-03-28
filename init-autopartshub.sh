#!/bin/bash
# ============================================================
# AutoPartsHub — Scaffold completo da solution
# Execute na raiz do repositório clonado:
#   bash init-autopartshub.sh
# ============================================================

set -e

echo "🚗 AutoPartsHub — Criando estrutura da solution..."
echo "==================================================="

# ── Verificar dependências ───────────────────────────────
check_cmd() {
  if ! command -v "$1" &> /dev/null; then
    echo "❌ '$1' não encontrado. Instale antes de continuar."
    exit 1
  fi
}
check_cmd dotnet
check_cmd node
check_cmd npm
check_cmd git

DOTNET_VERSION=$(dotnet --version)
echo "✅ .NET $DOTNET_VERSION"

# ── Criar Solution ───────────────────────────────────────
echo ""
echo "📦 Criando Solution..."
dotnet new sln --name AutoPartsHub --output .

# ── Projetos src/ ────────────────────────────────────────
echo "📦 Criando projetos backend..."

dotnet new webapi   --name AutoPartsHub.API         --output src/AutoPartsHub.API         --no-openapi false
dotnet new classlib --name AutoPartsHub.Application  --output src/AutoPartsHub.Application
dotnet new classlib --name AutoPartsHub.Domain       --output src/AutoPartsHub.Domain
dotnet new classlib --name AutoPartsHub.Infra        --output src/AutoPartsHub.Infra

# ── Projetos frontend/ ───────────────────────────────────
echo "📦 Criando projetos Blazor..."

dotnet new blazorserver --name AutoPartsHub.Blazor.Supplier --output frontend/blazor-supplier
dotnet new blazorserver --name AutoPartsHub.Blazor.Admin    --output frontend/blazor-admin

# ── Projetos tests/ ─────────────────────────────────────
echo "📦 Criando projetos de testes..."

dotnet new xunit --name AutoPartsHub.UnitTests        --output tests/AutoPartsHub.UnitTests
dotnet new xunit --name AutoPartsHub.IntegrationTests --output tests/AutoPartsHub.IntegrationTests

# ── Adicionar projetos à Solution ───────────────────────
echo "🔗 Adicionando projetos à solution..."

dotnet sln add src/AutoPartsHub.API/AutoPartsHub.API.csproj
dotnet sln add src/AutoPartsHub.Application/AutoPartsHub.Application.csproj
dotnet sln add src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj
dotnet sln add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj
dotnet sln add frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj
dotnet sln add frontend/blazor-admin/AutoPartsHub.Blazor.Admin.csproj
dotnet sln add tests/AutoPartsHub.UnitTests/AutoPartsHub.UnitTests.csproj
dotnet sln add tests/AutoPartsHub.IntegrationTests/AutoPartsHub.IntegrationTests.csproj

# ── Referências entre projetos ───────────────────────────
echo "🔗 Configurando referências entre projetos..."

# API depende de Application e Infra
dotnet add src/AutoPartsHub.API/AutoPartsHub.API.csproj \
  reference src/AutoPartsHub.Application/AutoPartsHub.Application.csproj
dotnet add src/AutoPartsHub.API/AutoPartsHub.API.csproj \
  reference src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj

# Application depende de Domain
dotnet add src/AutoPartsHub.Application/AutoPartsHub.Application.csproj \
  reference src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj

# Infra depende de Application e Domain
dotnet add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj \
  reference src/AutoPartsHub.Application/AutoPartsHub.Application.csproj
dotnet add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj \
  reference src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj

# Blazors dependem de Application e Infra
dotnet add frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj \
  reference src/AutoPartsHub.Application/AutoPartsHub.Application.csproj
dotnet add frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj \
  reference src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj

dotnet add frontend/blazor-admin/AutoPartsHub.Blazor.Admin.csproj \
  reference src/AutoPartsHub.Application/AutoPartsHub.Application.csproj
dotnet add frontend/blazor-admin/AutoPartsHub.Blazor.Admin.csproj \
  reference src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj

# Testes dependem de todos
dotnet add tests/AutoPartsHub.UnitTests/AutoPartsHub.UnitTests.csproj \
  reference src/AutoPartsHub.Application/AutoPartsHub.Application.csproj
dotnet add tests/AutoPartsHub.UnitTests/AutoPartsHub.UnitTests.csproj \
  reference src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj

dotnet add tests/AutoPartsHub.IntegrationTests/AutoPartsHub.IntegrationTests.csproj \
  reference src/AutoPartsHub.API/AutoPartsHub.API.csproj
dotnet add tests/AutoPartsHub.IntegrationTests/AutoPartsHub.IntegrationTests.csproj \
  reference src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj

# ── NuGet packages ───────────────────────────────────────
echo "📦 Instalando pacotes NuGet..."

# Domain
dotnet add src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj \
  package FluentResults

# Application
dotnet add src/AutoPartsHub.Application/AutoPartsHub.Application.csproj \
  package MediatR
dotnet add src/AutoPartsHub.Application/AutoPartsHub.Application.csproj \
  package FluentValidation.DependencyInjectionExtensions
dotnet add src/AutoPartsHub.Application/AutoPartsHub.Application.csproj \
  package FluentResults
dotnet add src/AutoPartsHub.Application/AutoPartsHub.Application.csproj \
  package Mapster

# Infra
dotnet add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj \
  package Microsoft.EntityFrameworkCore
dotnet add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj \
  package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj \
  package Microsoft.EntityFrameworkCore.Design
dotnet add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj \
  package StackExchange.Redis

# API
dotnet add src/AutoPartsHub.API/AutoPartsHub.API.csproj \
  package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/AutoPartsHub.API/AutoPartsHub.API.csproj \
  package Microsoft.AspNetCore.SignalR.StackExchangeRedis
dotnet add src/AutoPartsHub.API/AutoPartsHub.API.csproj \
  package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/AutoPartsHub.API/AutoPartsHub.API.csproj \
  package Serilog.AspNetCore

# Blazors
dotnet add frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj \
  package MudBlazor
dotnet add frontend/blazor-admin/AutoPartsHub.Blazor.Admin.csproj \
  package MudBlazor

# Testes
dotnet add tests/AutoPartsHub.UnitTests/AutoPartsHub.UnitTests.csproj \
  package FluentAssertions
dotnet add tests/AutoPartsHub.UnitTests/AutoPartsHub.UnitTests.csproj \
  package NSubstitute

dotnet add tests/AutoPartsHub.IntegrationTests/AutoPartsHub.IntegrationTests.csproj \
  package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/AutoPartsHub.IntegrationTests/AutoPartsHub.IntegrationTests.csproj \
  package Testcontainers.PostgreSql
dotnet add tests/AutoPartsHub.IntegrationTests/AutoPartsHub.IntegrationTests.csproj \
  package FluentAssertions

# ── Angular portal ───────────────────────────────────────
echo "📦 Criando Angular portal..."

if command -v npx &> /dev/null; then
  npx --yes @angular/cli@17 new angular-portal \
    --directory frontend/angular-portal \
    --routing true \
    --style scss \
    --standalone true \
    --skip-git \
    --skip-install

  echo "📦 Instalando dependências Angular..."
  cd frontend/angular-portal
  npm install
  npm install @ngrx/store @ngrx/effects @ngrx/entity @ngrx/store-devtools
  npm install @microsoft/signalr
  npm install @angular/material @angular/cdk
  cd ../..
else
  echo "⚠️  npx não encontrado — Angular portal não criado. Rode manualmente depois."
fi

# ── appsettings.Example.json ─────────────────────────────
echo "📄 Criando appsettings.Example.json..."

cat > src/AutoPartsHub.API/appsettings.Example.json << 'EOF'
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=autopartshub;Username=postgres;Password=postgres"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Jwt": {
    "Secret": "TROQUE-POR-UMA-CHAVE-SECRETA-DE-PELO-MENOS-32-CHARS",
    "Issuer": "autopartshub",
    "Audience": "autopartshub-clients",
    "ExpiresInMinutes": 60
  },
  "Sentry": {
    "Dsn": ""
  }
}
EOF

cp src/AutoPartsHub.API/appsettings.Example.json frontend/blazor-supplier/appsettings.Example.json
cp src/AutoPartsHub.API/appsettings.Example.json frontend/blazor-admin/appsettings.Example.json

# ── Dockerfiles ──────────────────────────────────────────
echo "🐳 Criando Dockerfiles..."

cat > src/AutoPartsHub.API/Dockerfile << 'EOF'
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AutoPartsHub.API/AutoPartsHub.API.csproj", "src/AutoPartsHub.API/"]
COPY ["src/AutoPartsHub.Application/AutoPartsHub.Application.csproj", "src/AutoPartsHub.Application/"]
COPY ["src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj", "src/AutoPartsHub.Domain/"]
COPY ["src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj", "src/AutoPartsHub.Infra/"]
RUN dotnet restore "src/AutoPartsHub.API/AutoPartsHub.API.csproj"
COPY . .
RUN dotnet build "src/AutoPartsHub.API/AutoPartsHub.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/AutoPartsHub.API/AutoPartsHub.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoPartsHub.API.dll"]
EOF

cat > frontend/blazor-supplier/Dockerfile << 'EOF'
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj", "frontend/blazor-supplier/"]
COPY ["src/AutoPartsHub.Application/AutoPartsHub.Application.csproj", "src/AutoPartsHub.Application/"]
COPY ["src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj", "src/AutoPartsHub.Domain/"]
COPY ["src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj", "src/AutoPartsHub.Infra/"]
RUN dotnet restore "frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj"
COPY . .
RUN dotnet build "frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoPartsHub.Blazor.Supplier.dll"]
EOF

cat > frontend/blazor-admin/Dockerfile << 'EOF'
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["frontend/blazor-admin/AutoPartsHub.Blazor.Admin.csproj", "frontend/blazor-admin/"]
COPY ["src/AutoPartsHub.Application/AutoPartsHub.Application.csproj", "src/AutoPartsHub.Application/"]
COPY ["src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj", "src/AutoPartsHub.Domain/"]
COPY ["src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj", "src/AutoPartsHub.Infra/"]
RUN dotnet restore "frontend/blazor-admin/AutoPartsHub.Blazor.Admin.csproj"
COPY . .
RUN dotnet build "frontend/blazor-admin/AutoPartsHub.Blazor.Admin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "frontend/blazor-admin/AutoPartsHub.Blazor.Admin.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoPartsHub.Blazor.Admin.dll"]
EOF

# ── husky + commitlint ───────────────────────────────────
echo "🐶 Configurando Husky + commitlint..."

npm init -y --silent 2>/dev/null || true
npm install --save-dev \
  @commitlint/cli \
  @commitlint/config-conventional \
  husky \
  --silent

npx husky install
npx husky add .husky/commit-msg 'npx --no -- commitlint --edit "$1"'

# ── Primeiro build para validar ──────────────────────────
echo ""
echo "🔨 Validando build da solution..."
dotnet build AutoPartsHub.sln --configuration Debug --verbosity minimal

# ── Resumo final ─────────────────────────────────────────
echo ""
echo "✅ Estrutura criada com sucesso!"
echo ""
echo "Estrutura final:"
echo ""
echo "  AutoPartsHub.sln"
echo "  ├── src/"
echo "  │   ├── AutoPartsHub.API"
echo "  │   ├── AutoPartsHub.Application"
echo "  │   ├── AutoPartsHub.Domain"
echo "  │   └── AutoPartsHub.Infra"
echo "  ├── frontend/"
echo "  │   ├── angular-portal"
echo "  │   ├── blazor-supplier"
echo "  │   └── blazor-admin"
echo "  └── tests/"
echo "      ├── AutoPartsHub.UnitTests"
echo "      └── AutoPartsHub.IntegrationTests"
echo ""
echo "Próximos passos:"
echo "  1. cp src/AutoPartsHub.API/appsettings.Example.json src/AutoPartsHub.API/appsettings.Development.json"
echo "  2. Preencha as connection strings no appsettings.Development.json"
echo "  3. docker compose up -d"
echo "  4. dotnet ef database update --project src/AutoPartsHub.Infra --startup-project src/AutoPartsHub.API"
echo "  5. git add . && git commit -m 'chore: scaffold inicial da solution' && git push"
