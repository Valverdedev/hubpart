# ============================================================
# AutoPartsHub — Scaffold completo da solution
# Execute na raiz do repositório clonado:
#   .\init-autopartshub.ps1
#
# Se der erro de permissão, rode antes:
#   Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
# ============================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "AutoPartsHub — Criando estrutura da solution..." -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan

# ── Verificar dependências ───────────────────────────────
function Check-Command($cmd) {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-Host "ERRO: '$cmd' nao encontrado. Instale antes de continuar." -ForegroundColor Red
        exit 1
    }
}

Check-Command dotnet
Check-Command node
Check-Command npm
Check-Command git

$dotnetVersion = dotnet --version
Write-Host "OK .NET $dotnetVersion" -ForegroundColor Green

# ── Criar Solution ───────────────────────────────────────
Write-Host ""
Write-Host "Criando Solution..." -ForegroundColor Yellow
dotnet new sln --name AutoPartsHub --output .

# ── Projetos src/ ────────────────────────────────────────
Write-Host "Criando projetos backend..." -ForegroundColor Yellow

dotnet new webapi   --name AutoPartsHub.API        --output src/AutoPartsHub.API        --no-openapi false
dotnet new classlib --name AutoPartsHub.Application --output src/AutoPartsHub.Application
dotnet new classlib --name AutoPartsHub.Domain      --output src/AutoPartsHub.Domain
dotnet new classlib --name AutoPartsHub.Infra       --output src/AutoPartsHub.Infra

# ── Projetos frontend/ ───────────────────────────────────
Write-Host "Criando projetos Blazor..." -ForegroundColor Yellow

dotnet new blazorserver --name AutoPartsHub.Blazor.Supplier --output frontend/blazor-supplier
dotnet new blazorserver --name AutoPartsHub.Blazor.Admin    --output frontend/blazor-admin

# ── Projetos tests/ ─────────────────────────────────────
Write-Host "Criando projetos de testes..." -ForegroundColor Yellow

dotnet new xunit --name AutoPartsHub.UnitTests        --output tests/AutoPartsHub.UnitTests
dotnet new xunit --name AutoPartsHub.IntegrationTests --output tests/AutoPartsHub.IntegrationTests

# ── Adicionar projetos à Solution ───────────────────────
Write-Host "Adicionando projetos a solution..." -ForegroundColor Yellow

dotnet sln add src/AutoPartsHub.API/AutoPartsHub.API.csproj
dotnet sln add src/AutoPartsHub.Application/AutoPartsHub.Application.csproj
dotnet sln add src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj
dotnet sln add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj
dotnet sln add frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj
dotnet sln add frontend/blazor-admin/AutoPartsHub.Blazor.Admin.csproj
dotnet sln add tests/AutoPartsHub.UnitTests/AutoPartsHub.UnitTests.csproj
dotnet sln add tests/AutoPartsHub.IntegrationTests/AutoPartsHub.IntegrationTests.csproj

# ── Referências entre projetos ───────────────────────────
Write-Host "Configurando referencias entre projetos..." -ForegroundColor Yellow

# API
dotnet add src/AutoPartsHub.API/AutoPartsHub.API.csproj reference src/AutoPartsHub.Application/AutoPartsHub.Application.csproj
dotnet add src/AutoPartsHub.API/AutoPartsHub.API.csproj reference src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj

# Application
dotnet add src/AutoPartsHub.Application/AutoPartsHub.Application.csproj reference src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj

# Infra
dotnet add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj reference src/AutoPartsHub.Application/AutoPartsHub.Application.csproj
dotnet add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj reference src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj

# Blazor Supplier
dotnet add frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj reference src/AutoPartsHub.Application/AutoPartsHub.Application.csproj
dotnet add frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj reference src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj

# Blazor Admin
dotnet add frontend/blazor-admin/AutoPartsHub.Blazor.Admin.csproj reference src/AutoPartsHub.Application/AutoPartsHub.Application.csproj
dotnet add frontend/blazor-admin/AutoPartsHub.Blazor.Admin.csproj reference src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj

# Testes
dotnet add tests/AutoPartsHub.UnitTests/AutoPartsHub.UnitTests.csproj reference src/AutoPartsHub.Application/AutoPartsHub.Application.csproj
dotnet add tests/AutoPartsHub.UnitTests/AutoPartsHub.UnitTests.csproj reference src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj
dotnet add tests/AutoPartsHub.IntegrationTests/AutoPartsHub.IntegrationTests.csproj reference src/AutoPartsHub.API/AutoPartsHub.API.csproj
dotnet add tests/AutoPartsHub.IntegrationTests/AutoPartsHub.IntegrationTests.csproj reference src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj

# ── NuGet packages ───────────────────────────────────────
Write-Host "Instalando pacotes NuGet..." -ForegroundColor Yellow

# Domain
dotnet add src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj package FluentResults

# Application
dotnet add src/AutoPartsHub.Application/AutoPartsHub.Application.csproj package MediatR
dotnet add src/AutoPartsHub.Application/AutoPartsHub.Application.csproj package FluentValidation.DependencyInjectionExtensions
dotnet add src/AutoPartsHub.Application/AutoPartsHub.Application.csproj package FluentResults
dotnet add src/AutoPartsHub.Application/AutoPartsHub.Application.csproj package Mapster

# Infra
dotnet add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj package Microsoft.EntityFrameworkCore
dotnet add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj package StackExchange.Redis

# API
dotnet add src/AutoPartsHub.API/AutoPartsHub.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/AutoPartsHub.API/AutoPartsHub.API.csproj package Microsoft.AspNetCore.SignalR.StackExchangeRedis
dotnet add src/AutoPartsHub.API/AutoPartsHub.API.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/AutoPartsHub.API/AutoPartsHub.API.csproj package Serilog.AspNetCore

# Blazors
dotnet add frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj package MudBlazor
dotnet add frontend/blazor-admin/AutoPartsHub.Blazor.Admin.csproj package MudBlazor

# Testes
dotnet add tests/AutoPartsHub.UnitTests/AutoPartsHub.UnitTests.csproj package FluentAssertions
dotnet add tests/AutoPartsHub.UnitTests/AutoPartsHub.UnitTests.csproj package NSubstitute
dotnet add tests/AutoPartsHub.IntegrationTests/AutoPartsHub.IntegrationTests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/AutoPartsHub.IntegrationTests/AutoPartsHub.IntegrationTests.csproj package Testcontainers.PostgreSql
dotnet add tests/AutoPartsHub.IntegrationTests/AutoPartsHub.IntegrationTests.csproj package FluentAssertions

# ── Angular portal ───────────────────────────────────────
Write-Host "Criando Angular portal..." -ForegroundColor Yellow

try {
    npx --yes @angular/cli@17 new angular-portal `
        --directory frontend/angular-portal `
        --routing true `
        --style scss `
        --standalone true `
        --skip-git `
        --skip-install

    Write-Host "Instalando dependencias Angular..." -ForegroundColor Yellow
    Set-Location frontend/angular-portal
    npm install
    npm install @ngrx/store @ngrx/effects @ngrx/entity @ngrx/store-devtools
    npm install @microsoft/signalr
    npm install @angular/material @angular/cdk
    Set-Location ../..
} catch {
    Write-Host "AVISO: Angular nao criado automaticamente. Crie manualmente depois." -ForegroundColor Yellow
}

# ── appsettings.Example.json ─────────────────────────────
Write-Host "Criando appsettings.Example.json..." -ForegroundColor Yellow

$appsettings = @'
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
'@

$appsettings | Set-Content src/AutoPartsHub.API/appsettings.Example.json -Encoding UTF8
Copy-Item src/AutoPartsHub.API/appsettings.Example.json frontend/blazor-supplier/appsettings.Example.json
Copy-Item src/AutoPartsHub.API/appsettings.Example.json frontend/blazor-admin/appsettings.Example.json

# ── Dockerfiles ──────────────────────────────────────────
Write-Host "Criando Dockerfiles..." -ForegroundColor Yellow

$dockerfileAPI = @'
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
'@

$dockerfileSupplier = @'
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
'@

$dockerfileAdmin = @'
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
'@

$dockerfileAPI      | Set-Content src/AutoPartsHub.API/Dockerfile -Encoding UTF8
$dockerfileSupplier | Set-Content frontend/blazor-supplier/Dockerfile -Encoding UTF8
$dockerfileAdmin    | Set-Content frontend/blazor-admin/Dockerfile -Encoding UTF8

# ── husky + commitlint ───────────────────────────────────
Write-Host "Configurando Husky + commitlint..." -ForegroundColor Yellow

npm init -y 2>$null | Out-Null
npm install --save-dev @commitlint/cli @commitlint/config-conventional husky --silent
npx husky install
npx husky add .husky/commit-msg 'npx --no -- commitlint --edit "$1"'

# ── Build de validação ───────────────────────────────────
Write-Host ""
Write-Host "Validando build da solution..." -ForegroundColor Yellow
dotnet build AutoPartsHub.sln --configuration Debug --verbosity minimal

# ── Resumo ───────────────────────────────────────────────
Write-Host ""
Write-Host "Estrutura criada com sucesso!" -ForegroundColor Green
Write-Host ""
Write-Host "  AutoPartsHub.sln"
Write-Host "  +-- src/"
Write-Host "  |   +-- AutoPartsHub.API"
Write-Host "  |   +-- AutoPartsHub.Application"
Write-Host "  |   +-- AutoPartsHub.Domain"
Write-Host "  |   +-- AutoPartsHub.Infra"
Write-Host "  +-- frontend/"
Write-Host "  |   +-- angular-portal"
Write-Host "  |   +-- blazor-supplier"
Write-Host "  |   +-- blazor-admin"
Write-Host "  +-- tests/"
Write-Host "      +-- AutoPartsHub.UnitTests"
Write-Host "      +-- AutoPartsHub.IntegrationTests"
Write-Host ""
Write-Host "Proximos passos:" -ForegroundColor Cyan
Write-Host "  1. Copie e preencha o appsettings:"
Write-Host "     cp src\AutoPartsHub.API\appsettings.Example.json src\AutoPartsHub.API\appsettings.Development.json"
Write-Host ""
Write-Host "  2. Suba a infra local:"
Write-Host "     docker compose up -d"
Write-Host ""
Write-Host "  3. Rode as migrations:"
Write-Host "     dotnet ef database update --project src\AutoPartsHub.Infra --startup-project src\AutoPartsHub.API"
Write-Host ""
Write-Host "  4. Commit inicial:"
Write-Host "     git add ."
Write-Host "     git commit -m 'chore: scaffold inicial da solution'"
Write-Host "     git push"
