namespace AutoPartsHub.Application.Interfaces;

public interface IEmailService
{
    Task EnviarVerificacaoEmailAsync(string email, string nomeCompleto, string token, CancellationToken ct);
    Task EnviarRetomadaCadastroAsync(string email, string sessionToken, CancellationToken ct);
    Task EnviarAlertaTrialAsync(string email, string nomeFantasia, string tipo, DateTime expiraEm, CancellationToken ct);
    Task EnviarTrialExpiradoAsync(string email, string nomeFantasia, CancellationToken ct);
}
