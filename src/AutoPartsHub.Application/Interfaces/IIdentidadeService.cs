using FluentResults;

namespace AutoPartsHub.Application.Interfaces;

/// <summary>DTO de usuário exposto pela camada de Application — sem dependência de Identity.</summary>
public record UsuarioDto(Guid Id, Guid TenantId, string Email, string NomeCompleto);

/// <summary>
/// Abstrai operações de identidade (UserManager) para desacoplar Application de ASP.NET Identity.
/// Implementação em Infra usando UserManager&lt;UsuarioApp&gt;.
/// </summary>
public interface IIdentidadeService
{
    Task<UsuarioDto?> BuscarPorEmailAsync(string email, CancellationToken ct = default);
    Task<UsuarioDto?> BuscarPorIdAsync(Guid usuarioId, CancellationToken ct = default);
    Task<bool> ValidarSenhaAsync(Guid usuarioId, string senha, CancellationToken ct = default);
    Task<IList<string>> ObterRolesAsync(Guid usuarioId, CancellationToken ct = default);
    Task AtualizarUltimoLoginAsync(Guid usuarioId, CancellationToken ct = default);
    Task<Result<Guid>> CriarUsuarioAsync(
        string nomeCompleto,
        string email,
        string senha,
        Guid tenantId,
        string role,
        CancellationToken ct = default);
}
