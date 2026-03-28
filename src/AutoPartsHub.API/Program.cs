using AutoPartsHub.API.Endpoints.Auth;
using AutoPartsHub.API.Extensions;
using AutoPartsHub.Application.Auth.Commands;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Infra.Identity;
using AutoPartsHub.Infra.Persistencia;
using AutoPartsHub.Infra.Repositorios;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- OpenAPI ---
builder.Services.AddOpenApi();

// --- Tenant Context (deve vir antes do DbContext pois AppDbContext depende de ITenantContext) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// --- Banco de dados ---
// EnsureCreated = false — o schema já existe no PostgreSQL
builder.Services.AddDbContext<AppDbContext>(opcoes =>
{
    opcoes.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
          .UseSnakeCaseNamingConvention(); // mapeia UserName → user_name, TenantId → tenant_id, etc.
});

// --- Identity + JWT ---
builder.Services.AdicionarIdentity(builder.Configuration);

// --- CQRS com MediatR ---
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly));

// --- Validação com FluentValidation ---
builder.Services.AddValidatorsFromAssembly(typeof(LoginCommand).Assembly);

// --- Repositórios ---
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// --- Pipeline de validação automática no MediatR ---
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidacaoBehavior<,>));

var app = builder.Build();

// --- Middlewares ---
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// --- Endpoints de autenticação ---
app.MaparRegistro();
app.MaparLogin();
app.MaparRefreshToken();

app.Run();

// ---------------------------------------------------------------------------
// Pipeline behavior para validação automática via FluentValidation
// ---------------------------------------------------------------------------

/// <summary>
/// Intercepta todos os commands/queries e executa os validators antes do handler.
/// Quando TResponse é Result ou Result&lt;T&gt;, retorna Result.Fail com as mensagens
/// de validação — nunca lança exceção (regra do CLAUDE.md).
/// </summary>
internal sealed class ValidacaoBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validadores
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!validadores.Any())
            return await next(ct);

        var contexto = new ValidationContext<TRequest>(request);
        var mensagensDeErro = validadores
            .Select(v => v.Validate(contexto))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => f.ErrorMessage)
            .ToList();

        if (mensagensDeErro.Count == 0)
            return await next(ct);

        // Retorna Result.Fail quando TResponse é Result<T> — sem throw
        var tipoResposta = typeof(TResponse);
        if (tipoResposta.IsGenericType && tipoResposta.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var tipoValor = tipoResposta.GetGenericArguments()[0];
            var metodo = typeof(Result)
                .GetMethods()
                .First(m => m.Name == "Fail" && m.IsGenericMethod && m.GetParameters().Length == 1
                            && m.GetParameters()[0].ParameterType == typeof(string))
                .MakeGenericMethod(tipoValor);

            return (TResponse)metodo.Invoke(null, [string.Join(" | ", mensagensDeErro)])!;
        }

        // Para Result não genérico
        if (tipoResposta == typeof(Result))
            return (TResponse)(object)Result.Fail(string.Join(" | ", mensagensDeErro));

        // Fallback para outros tipos de retorno (não deveria ocorrer no projeto)
        throw new ValidationException(string.Join(" | ", mensagensDeErro));
    }
}
