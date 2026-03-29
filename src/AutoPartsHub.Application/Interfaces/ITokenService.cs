namespace AutoPartsHub.Application.Interfaces;

/// <summary>
/// Abstrai a geração de tokens JWT e refresh tokens.
/// Implementação em Infra — Application só depende desta interface.
/// </summary>
public interface ITokenService
{
    /// <summary>Gera um JWT assinado com as claims obrigatórias do projeto.</summary>
    (string Token, DateTime ExpiraEm) GerarJwt(
        Guid usuarioId,
        Guid tenantId,
        string email,
        string nomeCompleto,
        IList<string> roles);

    /// <summary>
    /// Gera um valor bruto criptograficamente seguro para uso como refresh token
    /// (32 bytes → 44 chars base64). Este valor é retornado ao cliente e NUNCA persistido.
    /// </summary>
    string GerarRefreshToken();

    /// <summary>
    /// Calcula o hash SHA-256 (hex lowercase) do valor bruto do refresh token.
    /// Apenas o hash é persistido no banco.
    /// </summary>
    string HashRefreshToken(string valorBruto);
}
