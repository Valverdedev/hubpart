namespace AutoPartsHub.Domain.Entidades;

/// <summary>
/// Representa um refresh token emitido para um usuário autenticado.
/// Cada token é de uso único — após utilizado, é invalidado (rotação obrigatória).
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Valor do token (gerado aleatoriamente, armazenado em texto puro).</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>ID do usuário dono deste token.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Tenant ao qual este token pertence (segurança adicional).</summary>
    public Guid TenantId { get; set; }

    /// <summary>Data e hora UTC em que o token expira.</summary>
    public DateTime ExpiraEm { get; set; }

    /// <summary>Data e hora UTC em que o token foi utilizado (null = ainda não usado).</summary>
    public DateTime? UsadoEm { get; set; }

    /// <summary>Indica se o token foi revogado manualmente (logout, troca de senha, etc.).</summary>
    public bool Revogado { get; set; }

    /// <summary>Verifica se o token ainda é válido para uso.</summary>
    public bool EstaValido => !Revogado && UsadoEm is null && ExpiraEm > DateTime.UtcNow;
}
