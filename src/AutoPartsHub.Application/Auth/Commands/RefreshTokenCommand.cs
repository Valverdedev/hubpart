using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using FluentResults;
using FluentValidation;

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
    IIdentidadeService identidade,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IDateTimeProvider dateTime
) : ICommandHandler<RefreshTokenCommand, LoginResultadoDto>
{
    public async Task<Result<LoginResultadoDto>> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        // 1. Hasheia o valor recebido e busca pelo hash (nunca pelo valor bruto)
        var hash = tokenService.HashRefreshToken(command.RefreshToken);
        var tokenExistente = await refreshTokenRepository.ObterPorHashAsync(hash, ct);

        if (tokenExistente is null || !tokenExistente.EstaValido(dateTime))
            return Result.Fail<LoginResultadoDto>("token_invalido");

        // 2. Localiza o usuário dono do token
        var usuario = await identidade.BuscarPorIdAsync(tokenExistente.UsuarioId, ct);
        if (usuario is null)
            return Result.Fail<LoginResultadoDto>("token_invalido");

        // 3. Garante que o token pertence ao mesmo tenant do usuário
        //    (defesa em profundidade — o token foi buscado sem filtro de tenant)
        if (tokenExistente.TenantId != usuario.TenantId)
            return Result.Fail<LoginResultadoDto>("token_invalido");

        // 4. Invalida o token antigo (rotação — one-time use)
        tokenExistente.MarcarComoUsado(dateTime);

        // 5. Gera novo par de tokens
        var roles = await identidade.ObterRolesAsync(usuario.Id, ct);
        var (novoToken, expiraEm) = tokenService.GerarJwt(
            usuario.Id, usuario.TenantId, usuario.Email, usuario.NomeCompleto, roles);

        var novoValorBruto = tokenService.GerarRefreshToken();
        var novoHash = tokenService.HashRefreshToken(novoValorBruto);

        var novoRefreshToken = RefreshToken.Criar(novoHash, usuario.Id, usuario.TenantId, dateTime);
        await refreshTokenRepository.AdicionarAsync(novoRefreshToken, ct);

        // 6. Persiste invalidação do antigo + criação do novo em uma única operação
        await refreshTokenRepository.SalvarAlteracoesAsync(ct);

        return Result.Ok(new LoginResultadoDto(
            Token: novoToken,
            RefreshToken: novoValorBruto,
            ExpiraEm: expiraEm,
            NomeCompleto: usuario.NomeCompleto,
            Roles: [.. roles]
        ));
    }
}
