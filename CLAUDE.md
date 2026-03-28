# AutoPartsHub — Contexto do Projeto

SaaS B2B de cotação de peças automotivas em tempo real.
Conecta oficinas, frotas e revendas a fornecedores locais.

## Stack

| Camada | Tecnologia |
|--------|-----------|
| API | ASP.NET Core 8 — Minimal API, versionamento |
| Lógica | CQRS com MediatR + FluentValidation |
| Auth | ASP.NET Identity + JWT + Refresh Token |
| Tempo real | SignalR + Redis backplane (sem Azure SignalR) |
| ORM | EF Core 8, Code First, repositórios por aggregate |
| Banco | PostgreSQL 16 + PostGIS |
| Cache/Filas | Redis via Upstash |
| Frontend comprador | Angular 17+ (Standalone, Signals, NgRx) |
| Frontend fornecedor | Blazor Server + MudBlazor |
| Mobile | PWA no MVP, MAUI depois |
| Jobs | Hangfire |
| Pagamentos | Pagar.me com split automático |
| Push | Firebase FCM |
| E-mail | AWS SES |
| SMS fallback | Twilio |
| Storage | S3 / Cloudflare R2 |
| Erros | Sentry |
| Deploy | AWS Elastic Beanstalk (API/Blazor), Amplify (Angular), RDS |

## Arquitetura da Solution

```
AutoPartsHub.sln
├── src/
│   ├── AutoPartsHub.API          # ASP.NET Core 8 — endpoints, SignalR Hub, middlewares
│   ├── AutoPartsHub.Application  # CQRS — commands, queries, handlers
│   ├── AutoPartsHub.Domain       # Entidades, aggregates, value objects, domain events
│   ├── AutoPartsHub.Infra        # EF Core, repositórios, Redis, PostGIS, integrações
│   └── AutoPartsHub.Blazor       # Blazor Server — painel do fornecedor
├── frontend/
│   └── angular-portal            # Angular 17+ — portal oficinas, frotas, admin
└── tests/
    ├── AutoPartsHub.UnitTests
    └── AutoPartsHub.IntegrationTests
```

## Regras de código

- Classes, métodos, variáveis e comentários em **português**
- Exceções: palavras reservadas do C# e nomes de frameworks permanecem em inglês (ex: Handler, Command, Query, Repository)
- Sempre usar **Result pattern** para retorno de erros (nunca throw em handlers)
- **Multi-tenancy obrigatório**: toda query deve respeitar o `tenant_id`
- EF Core Global Query Filter filtra por tenant automaticamente
- `ITenantContext` injetado como Scoped — nunca acessar tenant_id direto do JWT no handler
- PostgreSQL RLS como segunda barreira de isolamento

## Multi-tenancy — regras críticas

```csharp
// ✅ CORRETO — usar ITenantContext injetado
public class GetQuotationHandler(IQuotationRepository repo, ITenantContext tenant)
{
    public async Task<Result<QuotationDto>> Handle(GetQuotationQuery query, CancellationToken ct)
    {
        // Global Query Filter já filtra por tenant_id automaticamente
        var quotation = await repo.GetByIdAsync(query.Id, ct);
        return quotation is null ? Result.Fail("not_found") : Result.Ok(quotation.ToDto());
    }
}

// ❌ ERRADO — não acessar JWT claims diretamente no handler
var tenantId = httpContext.User.FindFirst("tenant_id")?.Value;
```

## Result pattern

```csharp
// Sempre retornar Result<T> nos handlers — nunca lançar exceções de negócio
public async Task<Result<Guid>> Handle(CreateQuotationCommand cmd, CancellationToken ct)
{
    if (cmd.Parts.Count == 0)
        return Result.Fail<Guid>("parts_required");

    var quotation = Quotation.Create(cmd.BuyerId, cmd.Parts, _tenant.TenantId);
    await _repo.AddAsync(quotation, ct);
    return Result.Ok(quotation.Id);
}
```

## Fluxo de cotação (core do produto)

1. Comprador abre cotação (placa, código ou descrição da peça)
2. API valida, identifica fornecedores no raio via PostGIS (`ST_DWithin`)
3. SignalR Hub notifica fornecedores conectados
4. Fornecedores respondem em até 15-30 min (preço, prazo, disponibilidade)
5. Plataforma rankeia por preço + prazo + reputação
6. Comprador aprova → OC gerada automaticamente
7. Pagar.me processa com split automático de comissão
8. Após confirmação de entrega → repasse liberado

## Planos e comissões

| Plano | Preço/mês | Cotações/mês | Comissão |
|-------|-----------|--------------|----------|
| Free/Trial | R$0 (30d) | 10 | — |
| Básico | R$149 | 50 | 3% |
| Profissional | R$349 | 200 | 2% |
| Enterprise | R$799+ | Ilimitado | 1,5% |

## Prioridades do MVP (Fase 1)

- [ ] PostgreSQL + EF Core + ASP.NET Core API
- [ ] Angular portal básico + Blazor painel fornecedor
- [ ] SignalR cotação em tempo real
- [ ] ASP.NET Identity + JWT + multi-tenancy
- [ ] Fluxo completo de cotação (criar → notificar → aprovar)

## Pontos críticos a validar em PR reviews

- `tenant_id` presente em toda query sem Global Query Filter
- Result pattern nos handlers (sem `throw` para erros de negócio)
- Nenhum dado de tenant diferente vazando entre requests
- Migrations incrementais (nunca recriar tabela existente)
- SignalR groups por tenant — nunca broadcast global
