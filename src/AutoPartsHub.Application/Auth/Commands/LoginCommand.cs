using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using FluentResults;
using FluentValidation;

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
public record LoginCommand(string Email, string Senha) : ICommand<LoginResultadoDto>;

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
/// Autentica o usuário, gera JWT com claims obrigatórias e persiste o hash do refresh token.
/// Segue o Result pattern — nunca lança exceções de negócio.
/// Persiste refresh token e atualiza último login em uma única transação (SaveChangesAsync único).
/// </summary>
public sealed class LoginCommandHandler(
    IIdentidadeService identidade,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IDateTimeProvider dateTime
) : ICommandHandler<LoginCommand, LoginResultadoDto>
{
    public async Task<Result<LoginResultadoDto>> Handle(LoginCommand command, CancellationToken ct)
    {
        // 1. Localiza o usuário pelo e-mail
        var usuario = await identidade.BuscarPorEmailAsync(command.Email, ct);
        if (usuario is null)
            return Result.Fail<LoginResultadoDto>("credenciais_invalidas");

        // 2. Valida a senha
        var senhaCorreta = await identidade.ValidarSenhaAsync(usuario.Id, command.Senha, ct);
        if (!senhaCorreta)
            return Result.Fail<LoginResultadoDto>("credenciais_invalidas");

        // 3. Obtém os roles do usuário
        var roles = await identidade.ObterRolesAsync(usuario.Id, ct);

        // 4. Gera o JWT
        var (token, expiraEm) = tokenService.GerarJwt(
            usuario.Id, usuario.TenantId, usuario.Email, usuario.NomeCompleto, roles);

        // 5. Gera refresh token — valor bruto retornado ao cliente, hash persistido no banco
        var valorBruto = tokenService.GerarRefreshToken();
        var hash = tokenService.HashRefreshToken(valorBruto);

        var refreshToken = RefreshToken.Criar(hash, usuario.Id, usuario.TenantId, dateTime);
        await refreshTokenRepository.AdicionarAsync(refreshToken, ct);

        // 6. Atualiza último login e persiste tudo em uma única operação
        await identidade.AtualizarUltimoLoginAsync(usuario.Id, ct);
        await refreshTokenRepository.SalvarAlteracoesAsync(ct);

        return Result.Ok(new LoginResultadoDto(
            Token: token,
            RefreshToken: valorBruto,
            ExpiraEm: expiraEm,
            NomeCompleto: usuario.NomeCompleto,
            Roles: [.. roles]
        ));
    }
}
