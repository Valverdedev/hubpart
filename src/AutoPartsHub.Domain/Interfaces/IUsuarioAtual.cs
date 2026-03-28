namespace AutoPartsHub.Domain.Interfaces;

/// <summary>
/// Expõe dados do usuário autenticado na requisição atual.
/// Análogo ao ITenantContext — extrai claims do JWT sem acesso direto ao HttpContext nos handlers.
/// Retorna Guid.Empty / null quando não há usuário autenticado.
/// </summary>
public interface IUsuarioAtual
{
    Guid Id { get; }
    string? Email { get; }
    bool Autenticado { get; }
}
