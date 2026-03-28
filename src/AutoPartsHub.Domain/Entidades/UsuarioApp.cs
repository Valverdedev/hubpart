using Microsoft.AspNetCore.Identity;

namespace AutoPartsHub.Domain.Entidades;

/// <summary>
/// Usuário da aplicação. Herda IdentityUser para integração com ASP.NET Identity.
/// Cada usuário pertence a um tenant (empresa/oficina/fornecedor).
/// </summary>
public class UsuarioApp : IdentityUser<Guid>
{
    /// <summary>Identificador do tenant ao qual este usuário pertence.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Nome completo do usuário.</summary>
    public string NomeCompleto { get; set; } = string.Empty;

    /// <summary>Telefone de contato (opcional).</summary>
    public string? Telefone { get; set; }

    /// <summary>
    /// Limite de valor em reais que este usuário pode aprovar em ordens de compra
    /// sem necessitar de aprovação superior.
    /// </summary>
    public decimal LimiteAprovacao { get; set; }

    /// <summary>Data e hora UTC do último login bem-sucedido.</summary>
    public DateTime? UltimoLoginEm { get; set; }
}
