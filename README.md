# AutoPartsHub

> SaaS B2B de cotação de peças automotivas em tempo real

Plataforma que conecta oficinas mecânicas, revendas e gestores de frota a fornecedores locais, com cotação em tempo real como diferencial central. O processo manual que demora horas é resolvido em 30 minutos.

## Stack

- **API**: ASP.NET Core 8 + CQRS (MediatR) + Clean Architecture
- **Frontend comprador**: Angular 17+ (Standalone Components, Signals, NgRx)
- **Frontend fornecedor**: Blazor Server + MudBlazor
- **Banco**: PostgreSQL 16 + PostGIS
- **Tempo real**: SignalR + Redis backplane
- **Pagamentos**: Pagar.me com split automático de comissão
- **Deploy**: AWS Elastic Beanstalk + Amplify

## Estrutura do projeto

```
AutoPartsHub.sln
├── src/
│   ├── AutoPartsHub.API          # Endpoints, SignalR Hub, middlewares
│   ├── AutoPartsHub.Application  # Commands, queries, handlers
│   ├── AutoPartsHub.Domain       # Entidades, aggregates, domain events
│   ├── AutoPartsHub.Infra        # EF Core, Redis, PostGIS, integrações
│   └── AutoPartsHub.Blazor       # Painel do fornecedor
├── frontend/
│   └── angular-portal            # Portal oficinas, frotas e admin
└── tests/
    ├── AutoPartsHub.UnitTests
    └── AutoPartsHub.IntegrationTests
```

## Desenvolvimento local

### Pré-requisitos

- .NET 8 SDK
- Node 20+
- Docker (para PostgreSQL + Redis locais)

### Setup

```bash
# Subir infraestrutura local
docker compose up -d

# Rodar migrations
dotnet ef database update --project src/AutoPartsHub.Infra --startup-project src/AutoPartsHub.API

# API
dotnet run --project src/AutoPartsHub.API

# Angular
cd frontend/angular-portal
npm install
npm start

# Blazor
dotnet run --project src/AutoPartsHub.Blazor
```

### Variáveis de ambiente

Copie `appsettings.Example.json` para `appsettings.Development.json` e preencha:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=autopartshub;Username=postgres;Password=postgres"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Jwt": {
    "Secret": "sua-chave-secreta-local-minimo-32-chars"
  }
}
```

## Convenções de commit

Usamos [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: adicionar cotação em lote
fix: corrigir isolamento de tenant no SignalR
refactor: extrair TenantMiddleware para classe própria
chore: atualizar dependências do Angular
```

## Fluxo de trabalho

1. Criar branch a partir de `develop`: `feat/nome-da-feature`
2. Abrir PR — o Claude revisa automaticamente
3. CI deve passar (build + testes)
4. Merge com squash
5. Deploy automático para staging
6. Deploy para produção requer aprovação manual

## Licença

Proprietário — todos os direitos reservados.
