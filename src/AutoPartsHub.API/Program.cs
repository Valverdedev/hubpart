using System.Threading.RateLimiting;
using AutoPartsHub.API.Extensions;
using AutoPartsHub.Application.Auth.Commands;
using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Infra.Common;
using AutoPartsHub.Infra.Identity;
using AutoPartsHub.Infra.Integracoes;
using AutoPartsHub.Infra.Jobs;
using AutoPartsHub.Infra.Persistencia;
using AutoPartsHub.Infra.Repositorios;
using AutoPartsHub.Infra.Servicos;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AdicionarSwagger();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IUsuarioAtual, UsuarioAtual>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

builder.Services.AddDbContext<AppDbContext>(opcoes =>
{
    opcoes.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
          .UseSnakeCaseNamingConvention();
});

builder.Services.AdicionarIdentity(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(LoginCommand).Assembly);

builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<ILocalizacaoRepository, LocalizacaoRepository>();
builder.Services.AddScoped<IOnboardingRascunhoRepository, OnboardingRascunhoRepository>();
builder.Services.AddScoped<ICotacaoUsoMensalRepository, CotacaoUsoMensalRepository>();

builder.Services.AddScoped<IIdentidadeService, IdentidadeService>();
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddScoped<ILocalizacaoService, LocalizacaoService>();

builder.Services.AddScoped<IEmailService, MockEmailService>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddStackExchangeRedisCache(opcoes =>
{
    opcoes.Configuration = builder.Configuration.GetSection("Redis")["ConnectionString"] ?? "localhost:6379";
});

builder.Services.AddHttpClient<ICnpjService, ReceitaWsCnpjService>(c =>
{
    c.BaseAddress = new Uri("https://receitaws.com.br/v1/");
    c.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<ICepService, ViaCepService>(c =>
{
    c.BaseAddress = new Uri("https://viacep.com.br/");
    c.Timeout = TimeSpan.FromSeconds(10);
});

var connectionString = builder.Configuration.GetConnectionString("Default")
                      ?? throw new InvalidOperationException("ConnectionStrings:Default nao configurada.");

builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<ExpirarTrialJob>();

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidacaoBehavior<,>));

builder.Services.AddRateLimiter(opcoes =>
{
    opcoes.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 10;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    opcoes.AddPolicy("onboarding-cnpj", contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    opcoes.AddPolicy("onboarding-cep", contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

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

app.UseHangfireDashboard("/hangfire");

app.MapControllers();

RecurringJob.AddOrUpdate<ExpirarTrialJob>(
    "expirar-trial",
    job => job.ExecutarAsync(),
    "0 2 * * *",
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

app.Run();

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
