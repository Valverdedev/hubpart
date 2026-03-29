using AutoPartsHub.Domain.Interfaces;

namespace AutoPartsHub.Domain.Entidades;

/// <summary>
/// Representa um refresh token emitido para um usuário autenticado.
/// Cada token é de uso único — após utilizado, é invalidado (rotação obrigatória).
///
/// Apenas o hash SHA-256 (hex) do valor bruto é persistido.
/// O valor bruto trafega apenas na resposta HTTP e nunca é armazenado.
/// </summary>
public class RefreshToken : EntidadeBase
{
    /// <summary>Hash SHA-256 (hex lowercase) do valor bruto do token.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>ID do usuário dono deste token.</summary>
    public Guid UsuarioId { get; private set; }

    /// <summary>Data e hora UTC em que o token expira.</summary>
    public DateTime ExpiraEm { get; private set; }

    /// <summary>Data e hora UTC em que o token foi utilizado (null = ainda não usado).</summary>
    public DateTime? UsadoEm { get; private set; }

    /// <summary>Indica se o token foi revogado manualmente (logout, troca de senha, etc.).</summary>
    public bool Revogado { get; private set; }

    /// <summary>Verifica se o token ainda é válido para uso.</summary>
    public bool EstaValido(IDateTimeProvider dateTime)
        => !Revogado && UsadoEm is null && ExpiraEm > dateTime.UtcNow;

    protected RefreshToken() { }

    /// <summary>Cria um novo refresh token com validade de 7 dias.</summary>
    public static RefreshToken Criar(
        string tokenHash,
        Guid usuarioId,
        Guid tenantId,
        IDateTimeProvider dateTime,
        int diasExpiracao = 7)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Hash do token não pode ser vazio.", nameof(tokenHash));

        if (usuarioId == Guid.Empty)
            throw new ArgumentException("UsuarioId não pode ser vazio.", nameof(usuarioId));

        var rt = new RefreshToken();
        rt.DefinirTenant(tenantId);
        rt.TokenHash = tokenHash;
        rt.UsuarioId = usuarioId;
        rt.ExpiraEm = dateTime.UtcNow.AddDays(diasExpiracao);
        return rt;
    }

    /// <summary>Marca o token como utilizado (rotação — invalida para uso futuro).</summary>
    public void MarcarComoUsado(IDateTimeProvider dateTime)
    {
        if (!EstaValido(dateTime))
            throw new InvalidOperationException("Token já está inválido.");

        UsadoEm = dateTime.UtcNow;
        MarcarComoAtualizado(dateTime);
    }

    /// <summary>Revoga o token manualmente (logout, troca de senha, etc.).</summary>
    public void Revogar(IDateTimeProvider dateTime)
    {
        if (Revogado)
            throw new InvalidOperationException("Token já está revogado.");

        Revogado = true;
        MarcarComoAtualizado(dateTime);
    }
}
