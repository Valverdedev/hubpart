using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
// DTOs
// ---------------------------------------------------------------------------

/// <summary>Dados retornados após login bem-sucedido.</summary>
public record LoginResultadoDto(
    string Token,
    string RefreshToken,
    DateTime ExpiraEm,
    string NomeCompleto,
    string[] Roles
);

// ---------------------------------------------------------------------------
// Command
// ---------------------------------------------------------------------------

/// <summary>Command de autenticação — recebe email e senha, retorna JWT + refresh token.</summary>
public record LoginCommand(string Email, string Senha) : IRequest<Result<LoginResultadoDto>>;

// ---------------------------------------------------------------------------
// Validator
// ---------------------------------------------------------------------------

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("email_obrigatorio")
            .EmailAddress().WithMessage("email_invalido");

        RuleFor(c => c.Senha)
            .NotEmpty().WithMessage("senha_obrigatoria")
            .MinimumLength(8).WithMessage("senha_minimo_8_caracteres");
    }
}

// ---------------------------------------------------------------------------
// Handler
// ---------------------------------------------------------------------------

/// <summary>
/// Autentica o usuário, gera JWT com claims obrigatórias e persiste o refresh token.
/// Segue o Result pattern — nunca lança exceções de negócio.
/// </summary>
public sealed class LoginCommandHandler(
    UserManager<UsuarioApp> userManager,
    IConfiguration configuracao,
    IRefreshTokenRepository refreshTokenRepository
) : IRequestHandler<LoginCommand, Result<LoginResultadoDto>>
{
    public async Task<Result<LoginResultadoDto>> Handle(LoginCommand command, CancellationToken ct)
    {
        // 1. Localiza o usuário pelo e-mail
        var usuario = await userManager.FindByEmailAsync(command.Email);
        if (usuario is null)
            return Result.Fail<LoginResultadoDto>("credenciais_invalidas");

        // 2. Valida a senha
        var senhaCorreta = await userManager.CheckPasswordAsync(usuario, command.Senha);
        if (!senhaCorreta)
            return Result.Fail<LoginResultadoDto>("credenciais_invalidas");

        // 3. Obtém os roles do usuário
        var roles = await userManager.GetRolesAsync(usuario);

        // 4. Gera o JWT
        var (token, expiraEm) = GerarJwt(usuario, roles);

        // 5. Gera e persiste o refresh token
        var refreshToken = await GerarRefreshTokenAsync(usuario, ct);

        // 6. Atualiza último login
        usuario.UltimoLoginEm = DateTime.UtcNow;
        await userManager.UpdateAsync(usuario);

        return Result.Ok(new LoginResultadoDto(
            Token: token,
            RefreshToken: refreshToken,
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

        // Claims obrigatórias conforme CLAUDE.md: sub, tenant_id, role, email
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("tenant_id", usuario.TenantId.ToString()),
        };

        // Adiciona um claim por role
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
        // Token criptograficamente seguro (256 bits → 44 chars em Base64)
        var bytes = RandomNumberGenerator.GetBytes(32);
        var valorToken = Convert.ToBase64String(bytes);

        var refreshToken = new RefreshToken
        {
            Token = valorToken,
            UsuarioId = usuario.Id,
            TenantId = usuario.TenantId,
            ExpiraEm = DateTime.UtcNow.AddDays(7),
        };

        await refreshTokenRepository.AdicionarAsync(refreshToken, ct);
        await refreshTokenRepository.SalvarAlteracoesAsync(ct);

        return valorToken;
    }
}
