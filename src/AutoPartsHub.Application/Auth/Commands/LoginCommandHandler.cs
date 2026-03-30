using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using FluentResults;

namespace AutoPartsHub.Application.Auth.Commands;

/// <summary>
/// Authenticates user, generates JWT and persists refresh token hash.
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
        var usuario = await identidade.BuscarPorEmailAsync(command.Email, ct);
        if (usuario is null)
            return Result.Fail<LoginResultadoDto>("credenciais_invalidas");

        var senhaCorreta = await identidade.ValidarSenhaAsync(usuario.Id, command.Senha, ct);
        if (!senhaCorreta)
            return Result.Fail<LoginResultadoDto>("credenciais_invalidas");

        var roles = await identidade.ObterRolesAsync(usuario.Id, ct);

        var (token, expiraEm) = tokenService.GerarJwt(
            usuario.Id, usuario.TenantId, usuario.Email, usuario.NomeCompleto, roles);

        var valorBruto = tokenService.GerarRefreshToken();
        var hash = tokenService.HashRefreshToken(valorBruto);

        var refreshToken = RefreshToken.Criar(hash, usuario.Id, usuario.TenantId, dateTime);
        await refreshTokenRepository.AdicionarAsync(refreshToken, ct);

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
