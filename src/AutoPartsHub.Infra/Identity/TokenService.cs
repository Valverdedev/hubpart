using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoPartsHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AutoPartsHub.Infra.Identity;

/// <summary>
/// Implementação de ITokenService. Gera JWTs e refresh tokens.
/// Reside em Infra para isolar dependências de JWT e IConfiguration da camada Application.
/// </summary>
public sealed class TokenService(IConfiguration configuracao) : ITokenService
{
    public (string Token, DateTime ExpiraEm) GerarJwt(
        Guid usuarioId,
        Guid tenantId,
        string email,
        string nomeCompleto,
        IList<string> roles)
    {
        var segredo = configuracao["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT secret não configurado.");

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(segredo));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var minutosExpiracao = configuracao.GetValue<int>("Jwt:ExpiresInMinutes", 60);
        var expiraEm = DateTime.UtcNow.AddMinutes(minutosExpiracao);

        // Claims obrigatórias conforme CLAUDE.md: sub, tenant_id, role, email
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("tenant_id", tenantId.ToString()),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: configuracao["Jwt:Issuer"],
            audience: configuracao["Jwt:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiraEm,
            signingCredentials: credenciais
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }

    public string GerarRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    public string HashRefreshToken(string valorBruto)
    {
        var bytes = Encoding.UTF8.GetBytes(valorBruto);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
