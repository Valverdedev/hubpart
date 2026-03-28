using AutoPartsHub.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AutoPartsHub.Infra.Identity;

/// <summary>
/// Implementação de ITenantContext que extrai o tenant_id do JWT da requisição HTTP.
/// Registrado como Scoped — cada request recebe sua própria instância.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    private readonly Lazy<Guid> _tenantId;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        // Lazy para evitar processamento desnecessário em requests sem token
        _tenantId = new Lazy<Guid>(() => ExtrairTenantId(httpContextAccessor.HttpContext));
    }

    public Guid TenantId => _tenantId.Value;

    private static Guid ExtrairTenantId(HttpContext? contexto)
    {
        if (contexto?.User?.Identity?.IsAuthenticated is not true)
            return Guid.Empty;

        var valor = contexto.User.FindFirst("tenant_id")?.Value;

        return Guid.TryParse(valor, out var tenantId)
            ? tenantId
            : Guid.Empty;
    }
}
