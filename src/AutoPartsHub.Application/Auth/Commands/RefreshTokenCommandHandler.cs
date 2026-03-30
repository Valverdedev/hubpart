using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using FluentResults;

namespace AutoPartsHub.Application.Auth.Commands;

public sealed class RefreshTokenCommandHandler(
    IIdentidadeService identidade,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IDateTimeProvider dateTime
) : ICommandHandler<RefreshTokenCommand, LoginResultadoDto>
{
    public async Task<Result<LoginResultadoDto>> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        var hash = tokenService.HashRefreshToken(command.RefreshToken);
        var tokenExistente = await refreshTokenRepository.ObterPorHashAsync(hash, ct);

        if (tokenExistente is null || !tokenExistente.EstaValido(dateTime))
            return Result.Fail<LoginResultadoDto>("token_invalido");

        var usuario = await identidade.BuscarPorIdAsync(tokenExistente.UsuarioId, ct);
        if (usuario is null)
            return Result.Fail<LoginResultadoDto>("token_invalido");

        if (tokenExistente.TenantId != usuario.TenantId)
            return Result.Fail<LoginResultadoDto>("token_invalido");

        tokenExistente.MarcarComoUsado(dateTime);

        var roles = await identidade.ObterRolesAsync(usuario.Id, ct);
        var (novoToken, expiraEm) = tokenService.GerarJwt(
            usuario.Id, usuario.TenantId, usuario.Email, usuario.NomeCompleto, roles);

        var novoValorBruto = tokenService.GerarRefreshToken();
        var novoHash = tokenService.HashRefreshToken(novoValorBruto);

        var novoRefreshToken = RefreshToken.Criar(novoHash, usuario.Id, usuario.TenantId, dateTime);
        await refreshTokenRepository.AdicionarAsync(novoRefreshToken, ct);

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
