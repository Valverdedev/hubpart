# TASK: API de Onboarding — Cadastro do Comprador

## Contexto

Leia o `CLAUDE.md` na raiz antes de começar. Este arquivo define as convenções
obrigatórias do projeto (Result pattern, CQRS, multi-tenancy, nomenclatura, etc.).

Leia também `docs/spec-cadastro-comprador.md` para entender o domínio completo
antes de escrever qualquer código.

Esta task implementa **somente a API** do fluxo de cadastro do comprador.
Frontend Angular, painel Blazor e integração Pagar.me estão fora do escopo.

---

## Restrições obrigatórias

- Todos os handlers retornam `Result<T>` — nunca `throw` para erros de negócio.
- Toda entidade multi-tenant tem `tenant_id` e está coberta pelo Global Query Filter.
- A tabela `onboarding_rascunho` é a única exceção — sem `tenant_id`, sem filtro.
- Idioma do código: classes e métodos em inglês, comentários em português.
- Nenhuma lógica de negócio nos controllers — apenas dispatch de commands/queries.
- Migrations incrementais — nunca recriar tabela existente.
- Não instalar pacotes NuGet não listados no `CLAUDE.md` sem justificar em comentário.

---

## Ordem de implementação

Siga exatamente esta ordem. Não pule etapas. Rode `dotnet build` ao fim de cada etapa.

---

### Etapa 1 — Domain

**Arquivo:** `src/AutoPartsHub.Domain/Enums/TipoComprador.cs`

```csharp
public enum TipoComprador
{
    OficinaCarro,
    OficinaMoto,
    Logista,
    Frotista,
    Outro
}
```

**Arquivo:** `src/AutoPartsHub.Domain/Enums/PlanoAssinatura.cs`

```csharp
public enum PlanoAssinatura
{
    Free,
    Basico,
    Profissional,
    Enterprise
}
```

**Arquivo:** `src/AutoPartsHub.Domain/Enums/StatusAssinatura.cs`

```csharp
public enum StatusAssinatura
{
    Trial,
    Free,
    Ativo,
    Bloqueado
}
```

**Arquivo:** `src/AutoPartsHub.Domain/Entidades/Tenant.cs`

Aggregate root. Campos obrigatórios:

```
Id                    Guid
NomeFantasia          string (max 200)
RazaoSocial           string (max 200)
Cnpj                  string (18) — único
TipoComprador         TipoComprador
PlanoAtual            PlanoAssinatura
StatusAssinatura      StatusAssinatura
TrialExpiraEm         DateTime? (null para Outro)
CotacoesLimiteMes     int
UsuariosLimite        int
InscricaoEstadual     string? (max 30)
TelefoneComercial     string (max 20)
ComoNosConheceu       string? (max 40)
DescricaoOutro        string? — somente tipo Outro
SegmentoFrota         string? — somente Frotista
QtdVeiculosEstimada   int? — somente Frotista
LimiteAprovacaoAdmin  decimal? — somente Frotista
Endereco              Endereco (value object)
CriadoEm             DateTime
AtualizadoEm         DateTime
```

Value object `Endereco`:

```
Cep           string (9)
Logradouro    string (max 200)
Numero        string (max 20)
Complemento   string? (max 100)
Bairro        string (max 100)
Cidade        string (max 100)
Estado        string (2)
```

Método de fábrica estático `Tenant.Criar(...)` — nunca construtor público.
O método define `CotacoesLimiteMes` e `UsuariosLimite` com base no plano escolhido:

| Plano        | Cotações/mês | Usuários |
|--------------|-------------|----------|
| Free         | 10          | 1        |
| Basico       | 50          | 2        |
| Profissional | 200         | 5        |
| Enterprise   | int.MaxValue | int.MaxValue |

Regras do método `Criar`:
- `TipoComprador.Outro`: `StatusAssinatura = Free`, `TrialExpiraEm = null`, plano fixo `Free`.
- Demais tipos: `StatusAssinatura = Trial`, `TrialExpiraEm = DateTime.UtcNow.AddDays(30)`.
- Validar CNPJ não nulo e com 14 dígitos numéricos (sem máscara).

**Arquivo:** `src/AutoPartsHub.Domain/Entidades/OnboardingRascunho.cs`

Entidade simples — não é aggregate, não tem tenant_id:

```
Id             Guid
SessionToken   Guid — único
TipoPerfil     TipoComprador
UltimoStep     int
Dados          string (JSONB serializado)
Email          string?
IpOrigem       string?
UserAgent      string?
CriadoEm      DateTime
AtualizadoEm  DateTime
```

**Arquivo:** `src/AutoPartsHub.Domain/Entidades/CotacaoUsoMensal.cs`

```
TenantId         Guid (parte da PK composta)
AnoMes           string (char 7, formato YYYY-MM — parte da PK composta)
TotalCotacoes    int
AtualizadoEm     DateTime
```

---

### Etapa 2 — Infra: Mapeamentos EF Core

Criar os mappings em `src/AutoPartsHub.Infra/Persistencia/Mappings/`.

**TenantMapping.cs**
- Tabela: `tenants`
- `Cnpj` tem índice único
- `Endereco` mapeado como owned entity (colunas na tabela `tenants`, prefixo `endereco_`)
- Enums persistidos como `string` (`.HasConversion<string>()`)
- SEM Global Query Filter — tenant não filtra a si mesmo

**OnboardingRascunhoMapping.cs**
- Tabela: `onboarding_rascunho`
- SEM Global Query Filter — tabela pública, fora do multi-tenancy
- `SessionToken` tem índice único
- `Email` tem índice parcial: `WHERE email IS NOT NULL`
- `Dados` mapeado como `string` com `HasColumnType("jsonb")`

**CotacaoUsoMensalMapping.cs**
- Tabela: `cotacao_uso_mensal`
- PK composta: `(TenantId, AnoMes)`
- SEM Global Query Filter — acesso controlado no handler, não no EF

Registrar os três mappings no `AppDbContext`:

```csharp
public DbSet<Tenant> Tenants => Set<Tenant>();
public DbSet<OnboardingRascunho> OnboardingRascunhos => Set<OnboardingRascunho>();
public DbSet<CotacaoUsoMensal> CotacaoUsoMensal => Set<CotacaoUsoMensal>();
```

---

### Etapa 3 — Migration

```bash
dotnet ef migrations add AddOnboardingComprador \
  --project src/AutoPartsHub.Infra \
  --startup-project src/AutoPartsHub.API
```

Revisar o arquivo gerado antes de aplicar. Confirmar:
- Tabela `tenants` criada com todos os campos de `Tenant` e `Endereco` (owned).
- Tabela `onboarding_rascunho` criada.
- Tabela `cotacao_uso_mensal` com PK composta `(tenant_id, ano_mes)`.
- Índices únicos em `tenants.cnpj` e `onboarding_rascunho.session_token`.
- Índice parcial em `onboarding_rascunho.email WHERE email IS NOT NULL`.

Aplicar:
```bash
dotnet ef database update \
  --project src/AutoPartsHub.Infra \
  --startup-project src/AutoPartsHub.API
```

---

### Etapa 4 — Application: Commands e Handlers

Criar em `src/AutoPartsHub.Application/Onboarding/`.

---

#### 4.1 IniciarOnboardingCommand

**Responsabilidade:** criar o rascunho e retornar o `sessionToken`.

Request:
```
TipoPerfil    TipoComprador
IpOrigem      string?
UserAgent     string?
```

Response: `Guid` (sessionToken)

Handler:
1. Cria `OnboardingRascunho` com `SessionToken = Guid.NewGuid()`, `UltimoStep = 1`.
2. Persiste via repositório.
3. Retorna `Result.Ok(sessionToken)`.

---

#### 4.2 AtualizarRascunhoCommand

**Responsabilidade:** atualizar os dados parciais e o `UltimoStep`.

Request:
```
SessionToken   Guid
Step           int
Dados          Dictionary<string, object>
Email          string? — preenchido quando Step >= 3
```

Handler:
1. Busca rascunho pelo `SessionToken` — `Result.Fail("rascunho_nao_encontrado")` se não existir.
2. Serializa `Dados` como JSON e atualiza `OnboardingRascunho.Dados`.
3. Atualiza `UltimoStep` se o step atual for maior que o salvo.
4. Se `Email` fornecido, atualiza `OnboardingRascunho.Email`.
5. Persiste e retorna `Result.Ok()`.

---

#### 4.3 CadastrarCompradorCommand

**Responsabilidade:** criar Tenant + UsuarioApp em uma única transação. É o command
principal — chamado pelo endpoint `/finalizar/{token}`.

Request:
```
SessionToken   Guid
PlanoEscolhido PlanoAssinatura
```

Handler — executar nesta ordem dentro de uma transação:

1. Buscar rascunho pelo `SessionToken`.
   - Não encontrado: verificar se CNPJ do cache Redis existe como tenant.
     - Encontrou tenant: `Result.Ok(new { TenantId, Status = "ja_cadastrado" })`.
     - Não encontrou: `Result.Fail("sessao_expirada")`.

2. Desserializar `rascunho.Dados` para extrair todos os campos.

3. Verificar se CNPJ já existe em `tenants` (unicidade).
   - Existe: `Result.Ok(new { TenantId = existente.Id, Status = "empresa_ja_cadastrada" })`.
   - Não há `Result.Fail` aqui — retorna Ok com status para o frontend redirecionar.

4. Criar `Tenant` via `Tenant.Criar(...)` com os dados do rascunho e o `PlanoEscolhido`.

5. Criar `UsuarioApp` via `UserManager<UsuarioApp>`:
   - `TenantId = tenant.Id`
   - `Email` e `UserName` = email do responsável
   - `NomeCompleto`, `Telefone` dos dados do rascunho
   - Role: `"Comprador"`
   - `LimiteAprovacao` = valor informado (Frotista) ou `0`

6. Persistir `Tenant` no banco.

7. Deletar `OnboardingRascunho` da tabela.

8. Publicar domain event `CompradorCadastradoEvent` via `IPublisher` (MediatR):
   ```
   TenantId       Guid
   Email          string
   NomeFantasia   string
   PlanoAtual     PlanoAssinatura
   TrialExpiraEm  DateTime?
   ```

9. Retornar `Result.Ok(new { TenantId = tenant.Id, Status = "criado" })`.

**Handler do evento `CompradorCadastradoEvent`:**
- Disparar e-mail de verificação via AWS SES (usar `IEmailService` — criar interface, implementação mock no MVP).
- Não bloquear o command em caso de falha no e-mail — logar e seguir.

---

#### 4.4 LimparRascunhosAntigosCommand

**Responsabilidade:** job Hangfire de limpeza. Chamado pelo `ExpirarTrialJob`.

Handler:
1. Buscar rascunhos com `CriadoEm < DateTime.UtcNow.AddDays(-7)`.
2. Para cada um com `Email != null`: verificar se e-mail de retomada já foi enviado
   (verificar flag em Redis: `retomada_enviada:{sessionToken}`). Se não enviado e
   `CriadoEm` entre D+2 e D+5, publicar `RascunhoAbandonadoEvent` para disparo de e-mail.
3. Deletar os rascunhos.
4. Retornar contagem de removidos.

---

#### 4.5 ExpirarTrialCommand

**Responsabilidade:** rebaixar tenants com trial expirado. Chamado pelo `ExpirarTrialJob`.

Handler — três queries independentes:

**Query D-7:** tenants onde `TrialExpiraEm BETWEEN now() AND now() + 7 days`
AND `StatusAssinatura = Trial`. Publicar `AlertaTrialEvent` com tipo `"D7"`.

**Query D-1:** tenants onde `TrialExpiraEm BETWEEN now() AND now() + 1 day`
AND `StatusAssinatura = Trial`. Publicar `AlertaTrialEvent` com tipo `"D1"`.

**Query expirados:** tenants onde `StatusAssinatura = Trial AND TrialExpiraEm < now()`.
Para cada um:
1. Atualizar: `StatusAssinatura = Free`, `PlanoAtual = Free`,
   `CotacoesLimiteMes = 10`, `UsuariosLimite = 1`.
2. Invalidar cache Redis: `DEL tenant:{tenantId}`.
3. Publicar `TrialExpiradoEvent` com `TenantId` e `Email` do admin.

Retornar contagem de cada operação.

---

### Etapa 5 — Infra: Repositórios

Criar interfaces em `AutoPartsHub.Application` e implementações em `AutoPartsHub.Infra`.

**ITenantRepository**
```csharp
Task<Tenant?> BuscarPorCnpjAsync(string cnpj, CancellationToken ct);
Task AdicionarAsync(Tenant tenant, CancellationToken ct);
Task<Tenant?> BuscarPorIdAsync(Guid id, CancellationToken ct);
```

**IOnboardingRascunhoRepository**
```csharp
Task<OnboardingRascunho?> BuscarPorTokenAsync(Guid sessionToken, CancellationToken ct);
Task AdicionarAsync(OnboardingRascunho rascunho, CancellationToken ct);
Task AtualizarAsync(OnboardingRascunho rascunho, CancellationToken ct);
Task DeletarAsync(OnboardingRascunho rascunho, CancellationToken ct);
Task<List<OnboardingRascunho>> BuscarAntigosAsync(DateTime antes, CancellationToken ct);
```

**ICotacaoUsoMensalRepository**
```csharp
Task<int> BuscarTotalAsync(Guid tenantId, string anoMes, CancellationToken ct);
Task IncrementarAsync(Guid tenantId, string anoMes, CancellationToken ct);
```

O método `IncrementarAsync` deve usar o upsert atômico via SQL raw:
```sql
INSERT INTO cotacao_uso_mensal (tenant_id, ano_mes, total_cotacoes, atualizado_em)
VALUES (@tenantId, @anoMes, 1, now())
ON CONFLICT (tenant_id, ano_mes)
DO UPDATE SET
  total_cotacoes = cotacao_uso_mensal.total_cotacoes + 1,
  atualizado_em  = now();
```

---

### Etapa 6 — Infra: Serviços externos

**IEmailService** — interface em `AutoPartsHub.Application`:
```csharp
Task EnviarVerificacaoEmailAsync(string email, string nomeCompleto, string token, CancellationToken ct);
Task EnviarRetomadaCadastroAsync(string email, string sessionToken, CancellationToken ct);
Task EnviarAlertaTrialAsync(string email, string nomeFantasia, string tipo, DateTime expiraEm, CancellationToken ct);
Task EnviarTrialExpiradoAsync(string email, string nomeFantasia, CancellationToken ct);
```

**MockEmailService** — implementação em `AutoPartsHub.Infra/Servicos/`:
- Logar todas as chamadas via `ILogger` com nível `Information`.
- Não lançar exceção — simular envio bem-sucedido.
- Registrar no DI como `IEmailService` no MVP.

**ICnpjService** — interface em `AutoPartsHub.Application`:
```csharp
Task<CnpjInfoDto?> ConsultarAsync(string cnpj, CancellationToken ct);
```

`CnpjInfoDto`:
```
RazaoSocial    string
NomeFantasia   string?
Situacao       string
Logradouro     string
Numero         string
Complemento    string?
Bairro         string
Cidade         string
Estado         string
Cep            string
```

**ReceitaWsCnpjService** — implementação em `AutoPartsHub.Infra/Servicos/`:
- Usar `HttpClient` via `IHttpClientFactory`.
- URL: `https://receitaws.com.br/v1/cnpj/{cnpj}`.
- Retornar `null` se CNPJ inativo (`situacao != "ATIVA"`) ou erro HTTP.
- Registrar como `ICnpjService` no DI.

**ICepService** — interface em `AutoPartsHub.Application`:
```csharp
Task<CepInfoDto?> ConsultarAsync(string cep, CancellationToken ct);
```

**ViaCepService** — implementação em `AutoPartsHub.Infra/Servicos/`:
- URL: `https://viacep.com.br/ws/{cep}/json/`.
- Retornar `null` se CEP inválido (campo `erro: true` na resposta).

---

### Etapa 7 — Infra: Jobs Hangfire

**Arquivo:** `src/AutoPartsHub.Infra/Jobs/ExpirarTrialJob.cs`

```csharp
public class ExpirarTrialJob(ISender sender, ILogger<ExpirarTrialJob> logger)
{
    [JobDisplayName("Expirar trial e alertas")]
    public async Task ExecutarAsync()
    {
        var resultado = await sender.Send(new ExpirarTrialCommand());
        logger.LogInformation(
            "ExpirarTrialJob: {Expirados} expirados, {D7} alertas D-7, {D1} alertas D-1",
            resultado.Value.Expirados, resultado.Value.AlertasD7, resultado.Value.AlertasD1);

        await sender.Send(new LimparRascunhosAntigosCommand());
    }
}
```

Registrar o job recorrente no startup (`Program.cs`):
```csharp
RecurringJob.AddOrUpdate<ExpirarTrialJob>(
    "expirar-trial",
    job => job.ExecutarAsync(),
    "0 2 * * *", // diário às 02h00 UTC
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
```

---

### Etapa 8 — API: Controllers e Endpoints

Criar `src/AutoPartsHub.API/Controllers/OnboardingController.cs`.

Todos os endpoints são públicos (sem `[Authorize]`).
Rate limiting por IP via `AspNetCoreRateLimit` — instalar se não presente.

#### Endpoints

**GET /api/v1/onboarding/cnpj/{cnpj}**
- Chama `ICnpjService.ConsultarAsync`.
- Retorna `404` se CNPJ inativo ou inválido.
- Retorna `200` com `CnpjInfoDto`.
- Rate limit: 30 req/min por IP.

**GET /api/v1/onboarding/cep/{cep}**
- Chama `ICepService.ConsultarAsync`.
- Retorna `404` se CEP inválido.
- Retorna `200` com `CepInfoDto`.
- Rate limit: 60 req/min por IP.

**POST /api/v1/onboarding/iniciar**

Request body:
```json
{ "tipoPerfil": "OficinaCarro", "ipOrigem": "...", "userAgent": "..." }
```

Response `201`:
```json
{ "sessionToken": "uuid" }
```

**PUT /api/v1/onboarding/rascunho/{sessionToken}**

Request body:
```json
{ "step": 2, "dados": { ... }, "email": "..." }
```

Response `200` ou `404` se token não encontrado.

**GET /api/v1/onboarding/rascunho/{sessionToken}**

Response `200`:
```json
{ "tipoPerfil": "...", "ultimoStep": 2, "dados": { ... } }
```
Response `404` se não encontrado (expirado ou já finalizado).

**POST /api/v1/onboarding/finalizar/{sessionToken}**

Request body:
```json
{ "planoEscolhido": "Profissional" }
```

Response `200` em todos os casos (idempotente):
```json
{
  "tenantId": "uuid",
  "status": "criado" | "ja_cadastrado" | "empresa_ja_cadastrada",
  "redirectTo": "/login"
}
```

Response `400` apenas para `sessao_expirada`:
```json
{ "erro": "sessao_expirada", "mensagem": "Seu rascunho expirou. Inicie um novo cadastro." }
```

**POST /api/v1/onboarding/reenviar-verificacao**

Request body:
```json
{ "email": "..." }
```

Response `200` sempre (não confirmar se e-mail existe — segurança).
Rate limit Redis: 3 reenvios/hora por e-mail (chave `reenvio:{email}`).

---

### Etapa 9 — Testes

Criar em `tests/AutoPartsHub.UnitTests/Onboarding/`.

Cobrir obrigatoriamente:

**Tenant.Criar:**
- Tipo `Outro` → `StatusAssinatura.Free`, `TrialExpiraEm = null`, `PlanoAtual = Free`.
- Tipo `OficinaCarro` → `StatusAssinatura.Trial`, `TrialExpiraEm` em ~30 dias.
- Plano `Profissional` → `CotacoesLimiteMes = 200`, `UsuariosLimite = 5`.
- Plano `Enterprise` → `CotacoesLimiteMes = int.MaxValue`.

**CadastrarCompradorCommandHandler:**
- CNPJ já cadastrado → retorna `Ok` com `status = "empresa_ja_cadastrada"`.
- Rascunho não encontrado e sem cache → `Fail("sessao_expirada")`.
- Fluxo feliz → tenant criado, rascunho deletado, evento publicado.

**ExpirarTrialCommandHandler:**
- Tenant com trial expirado → `StatusAssinatura = Free`, cache Redis invalidado.
- Tenant D-7 → `AlertaTrialEvent` publicado com tipo `"D7"`.

**CotacaoUsoMensalRepository.IncrementarAsync:**
- Testar com `Testcontainers.PostgreSql` que o upsert é atômico.

---

### Etapa 10 — Registro no DI

Em `src/AutoPartsHub.API/Program.cs`, adicionar:

```csharp
// Repositórios de onboarding
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IOnboardingRascunhoRepository, OnboardingRascunhoRepository>();
builder.Services.AddScoped<ICotacaoUsoMensalRepository, CotacaoUsoMensalRepository>();

// Serviços externos
builder.Services.AddScoped<IEmailService, MockEmailService>();
builder.Services.AddHttpClient<ICnpjService, ReceitaWsCnpjService>();
builder.Services.AddHttpClient<ICepService, ViaCepService>();

// Hangfire
builder.Services.AddHangfire(config => config.UsePostgreSqlStorage(connectionString));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<ExpirarTrialJob>();
```

---

## Checklist de entrega

Antes de considerar a task concluída, verificar cada item:

- [ ] `dotnet build` sem warnings
- [ ] `dotnet test` todos os testes passando
- [ ] Migration aplicada e revisada
- [ ] Todos os endpoints respondem no Swagger (`/swagger`)
- [ ] `GET /api/v1/onboarding/cnpj/00000000000191` retorna dados da Receita Federal
- [ ] `POST /api/v1/onboarding/iniciar` cria rascunho e retorna sessionToken
- [ ] `POST /api/v1/onboarding/finalizar/{token}` cria tenant e usuário no banco
- [ ] Double submit retorna `200` com `status = "ja_cadastrado"`
- [ ] `ExpirarTrialJob` registrado no Hangfire dashboard (`/hangfire`)
- [ ] Nenhum `tenant_id` em `onboarding_rascunho` ou `cotacao_uso_mensal`
- [ ] Enums persistidos como string no banco (não como int)
- [ ] `MockEmailService` logando todas as chamadas sem lançar exceção

---

## O que NÃO implementar nesta task

- Frontend Angular (task separada)
- Integração Pagar.me (Fase 2)
- Autenticação JWT nos endpoints de onboarding (todos são públicos)
- Cadastro do fornecedor (spec separada)
- SesEmailService real (usar MockEmailService)
- Fluxo de upgrade/downgrade de plano (Fase 2)
