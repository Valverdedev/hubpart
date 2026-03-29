using AutoPartsHub.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Infra.Servicos;

/// <summary>
/// Implementação mock do IEmailService para o MVP.
/// Loga todas as chamadas via ILogger e não lança exceção.
/// Substituir por SesEmailService quando AWS SES estiver configurado.
/// </summary>
public sealed class MockEmailService(ILogger<MockEmailService> logger) : IEmailService
{
    public Task EnviarVerificacaoEmailAsync(string email, string nomeCompleto, string token, CancellationToken ct)
    {
        logger.LogInformation(
            "[MockEmail] Verificação de e-mail — Para: {Email}, Nome: {Nome}, Token: {Token}",
            email, nomeCompleto, token);
        return Task.CompletedTask;
    }

    public Task EnviarRetomadaCadastroAsync(string email, string sessionToken, CancellationToken ct)
    {
        logger.LogInformation(
            "[MockEmail] Retomada de cadastro — Para: {Email}, SessionToken: {Token}",
            email, sessionToken);
        return Task.CompletedTask;
    }

    public Task EnviarAlertaTrialAsync(string email, string nomeFantasia, string tipo, DateTime expiraEm, CancellationToken ct)
    {
        logger.LogInformation(
            "[MockEmail] Alerta trial {Tipo} — Para: {Email}, Empresa: {Empresa}, Expira: {Expira:O}",
            tipo, email, nomeFantasia, expiraEm);
        return Task.CompletedTask;
    }

    public Task EnviarTrialExpiradoAsync(string email, string nomeFantasia, CancellationToken ct)
    {
        logger.LogInformation(
            "[MockEmail] Trial expirado — Para: {Email}, Empresa: {Empresa}",
            email, nomeFantasia);
        return Task.CompletedTask;
    }
}
