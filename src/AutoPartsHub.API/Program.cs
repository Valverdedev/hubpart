using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using AutoPartsHub.API.Extensions;
using AutoPartsHub.Application.Auth.Commands;
using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Infra.Common;
using AutoPartsHub.Infra.Identity;
using AutoPartsHub.Infra.Integracoes;
using AutoPartsHub.Infra.Persistencia;
using AutoPartsHub.Infra.Repositorios;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Controllers + OpenAPI ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AdicionarSwagger();

// --- Infraestrutura de contexto (deve vir antes do DbContext) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IUsuarioAtual, UsuarioAtual>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

// --- Banco de dados ---
builder.Services.AddDbContext<AppDbContext>(opcoes =>
{
    opcoes.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
          .UseSnakeCaseNamingConvention();
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
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<ILocalizacaoRepository, LocalizacaoRepository>();

// --- Serviços de identidade e tokens ---
builder.Services.AddScoped<IIdentidadeService, IdentidadeService>();
builder.Services.AddSingleton<ITokenService, TokenService>();

// --- Serviços de domínio ---
builder.Services.AddScoped<ILocalizacaoService, LocalizacaoService>();

// --- Integrações externas ---
builder.Services.AddHttpClient<ICnpjService, CnpjService>(c =>
{
    c.BaseAddress = new Uri("https://open.cnpja.com/");
    c.Timeout = TimeSpan.FromSeconds(10);
});

// --- Pipeline behaviors (ordem importa: Logging → Validação → Handler) ---
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidacaoBehavior<,>));

// --- Rate limiting (proteção anti-abuso nos endpoints anônimos de auth) ---
builder.Services.AddRateLimiter(opcoes =>
{
    opcoes.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 10;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// --- Middlewares ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoPartsHub API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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

        var tipoResposta = typeof(TResponse);
        if (tipoResposta.IsGenericType && tipoResposta.GetGenericTypeDefinition() == typeof(FluentResults.Result<>))
        {
            var tipoValor = tipoResposta.GetGenericArguments()[0];
            var metodo = typeof(FluentResults.Result)
                .GetMethods()
                .First(m => m.Name == "Fail" && m.IsGenericMethod && m.GetParameters().Length == 1
                            && m.GetParameters()[0].ParameterType == typeof(string))
                .MakeGenericMethod(tipoValor);

            return (TResponse)metodo.Invoke(null, [string.Join(" | ", mensagensDeErro)])!;
        }

        if (tipoResposta == typeof(FluentResults.Result))
            return (TResponse)(object)FluentResults.Result.Fail(string.Join(" | ", mensagensDeErro));

        throw new FluentValidation.ValidationException(string.Join(" | ", mensagensDeErro));
    }
}
