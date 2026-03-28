namespace AutoPartsHub.Domain.Interfaces;

/// <summary>
/// Fornece o tenant_id do usuário autenticado na requisição atual.
/// Deve ser registrado como Scoped para que cada request tenha seu próprio contexto.
///
/// REGRA: nunca acessar JWT claims diretamente nos handlers —
/// sempre injetar ITenantContext.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Identificador do tenant extraído do JWT da requisição.
    /// Retorna Guid.Empty quando não há usuário autenticado.
    /// </summary>
    Guid TenantId { get; }
}
