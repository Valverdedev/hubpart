using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoPartsHub.Application.Common;
using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AutoPartsHub.Application.Auth.Commands;

// ---------------------------------------------------------------------------
// Command
// ---------------------------------------------------------------------------

/// <summary>
/// Renova o par JWT + refresh token a partir de um refresh token válido.
/// Implementa rotação: o token antigo é invalidado e um novo é emitido.
/// </summary>
public record RefreshTokenCommand(string RefreshToken) : ICommand<LoginResultadoDto>;

// ---------------------------------------------------------------------------
// Validator
// ---------------------------------------------------------------------------

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(c => c.RefreshToken)
            .NotEmpty().WithMessage("refresh_token_obrigatorio")
            .MaximumLength(512).WithMessage("refresh_token_invalido");
    }
}

// ---------------------------------------------------------------------------
// Handler
// ---------------------------------------------------------------------------

public sealed class RefreshTokenCommandHandler(
    UserManager<UsuarioApp> userManager,
    IConfiguration configuracao,
    IRefreshTokenRepository refreshTokenRepository,
    IDateTimeProvider dateTime
) : ICommandHandler<RefreshTokenCommand, LoginResultadoDto>
{
    public async Task<Result<LoginResultadoDto>> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        // 1. Busca o refresh token (Global Query Filter aplica filtro por tenant automaticamente)
        var tokenExistente = await refreshTokenRepository.ObterPorTokenAsync(command.RefreshToken, ct);

        if (tokenExistente is null || !tokenExistente.EstaValido(dateTime))
            return Result.Fail<LoginResultadoDto>("token_invalido");

        // 2. Localiza o usuário dono do token
        var usuario = await userManager.FindByIdAsync(tokenExistente.UsuarioId.ToString());
        if (usuario is null)
            return Result.Fail<LoginResultadoDto>("token_invalido");

        // 3. Garante que o token pertence ao mesmo tenant do usuário
        //    (defesa em profundidade — o token foi buscado sem filtro de tenant)
        if (tokenExistente.TenantId != usuario.TenantId)
            return Result.Fail<LoginResultadoDto>("token_invalido");

        // 4. Invalida o token antigo (rotação — one-time use)
        tokenExistente.MarcarComoUsado(dateTime);
        await refreshTokenRepository.SalvarAlteracoesAsync(ct);

        // 5. Obtém roles atualizados
        var roles = await userManager.GetRolesAsync(usuario);

        // 6. Gera novo JWT
        var (novoToken, expiraEm) = GerarJwt(usuario, roles);

        // 7. Gera e persiste novo refresh token
        var novoRefreshToken = await GerarRefreshTokenAsync(usuario, ct);

        return Result.Ok(new LoginResultadoDto(
            Token: novoToken,
            RefreshToken: novoRefreshToken,
            ExpiraEm: expiraEm,
            NomeCompleto: usuario.NomeCompleto,
            Roles: [.. roles]
        ));
    }

    private (string Token, DateTime ExpiraEm) GerarJwt(UsuarioApp usuario, IList<string> roles)
    {
        var segredo = configuracao["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT secret não configurado.");

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(segredo));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var minutosExpiracao = configuracao.GetValue<int>("Jwt:ExpiresInMinutes", 60);
        var expiraEm = DateTime.UtcNow.AddMinutes(minutosExpiracao);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("tenant_id", usuario.TenantId.ToString()),
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

    private async Task<string> GerarRefreshTokenAsync(UsuarioApp usuario, CancellationToken ct)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var valorToken = Convert.ToBase64String(bytes);

        var refreshToken = RefreshToken.Criar(valorToken, usuario.Id, usuario.TenantId, dateTime);

        await refreshTokenRepository.AdicionarAsync(refreshToken, ct);
        await refreshTokenRepository.SalvarAlteracoesAsync(ct);

        return valorToken;
    }
}
