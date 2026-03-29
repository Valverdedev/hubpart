using AutoPartsHub.Application.Auth.Commands;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace AutoPartsHub.UnitTests.Auth;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly IIdentidadeService _identidade = Substitute.For<IIdentidadeService>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IRefreshTokenRepository _refreshTokenRepo = Substitute.For<IRefreshTokenRepository>();
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();

    private RefreshTokenCommandHandler CriarHandler() =>
        new(_identidade, _tokenService, _refreshTokenRepo, _dateTime);

    [Fact]
    public async Task Handle_TokenValido_RotacionaERetornaNovoToken()
    {
        // Arrange
        var agora = DateTime.UtcNow;
        _dateTime.UtcNow.Returns(agora);

        var tenantId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var hashAntigo = "hash_antigo";

        var tokenExistente = RefreshToken.Criar(hashAntigo, usuarioId, tenantId, _dateTime);
        var usuario = new UsuarioDto(usuarioId, tenantId, "u@test.com", "Nome");
        var expiraEm = agora.AddHours(1);

        _tokenService.HashRefreshToken("valor_bruto_antigo").Returns(hashAntigo);
        _refreshTokenRepo.ObterPorHashAsync(hashAntigo, Arg.Any<CancellationToken>())
            .Returns(tokenExistente);
        _identidade.BuscarPorIdAsync(usuarioId, Arg.Any<CancellationToken>())
            .Returns(usuario);
        _identidade.ObterRolesAsync(usuarioId, Arg.Any<CancellationToken>())
            .Returns(new List<string> { "Comprador" });
        _tokenService.GerarJwt(usuarioId, tenantId, "u@test.com", "Nome", Arg.Any<IList<string>>())
            .Returns(("novo_jwt", expiraEm));
        _tokenService.GerarRefreshToken().Returns("novo_valor_bruto");
        _tokenService.HashRefreshToken("novo_valor_bruto").Returns("novo_hash");

        // Act
        var resultado = await CriarHandler().Handle(
            new RefreshTokenCommand("valor_bruto_antigo"), CancellationToken.None);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Token.Should().Be("novo_jwt");
        resultado.Value.RefreshToken.Should().Be("novo_valor_bruto");

        // Token antigo deve ter sido invalidado (UsadoEm preenchido)
        tokenExistente.EstaValido(_dateTime).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_TokenNaoEncontrado_RetornaFalha()
    {
        _tokenService.HashRefreshToken(Arg.Any<string>()).Returns("hash_qualquer");
        _refreshTokenRepo.ObterPorHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var resultado = await CriarHandler().Handle(
            new RefreshTokenCommand("token_invalido"), CancellationToken.None);

        resultado.IsFailed.Should().BeTrue();
        resultado.Errors[0].Message.Should().Be("token_invalido");
    }

    [Fact]
    public async Task Handle_TenantMismatch_RetornaFalha()
    {
        var agora = DateTime.UtcNow;
        _dateTime.UtcNow.Returns(agora);

        var tenantToken = Guid.NewGuid();
        var tenantUsuario = Guid.NewGuid(); // diferente!
        var usuarioId = Guid.NewGuid();
        var hash = "algum_hash";

        var tokenExistente = RefreshToken.Criar(hash, usuarioId, tenantToken, _dateTime);
        var usuario = new UsuarioDto(usuarioId, tenantUsuario, "u@test.com", "Nome");

        _tokenService.HashRefreshToken("valor").Returns(hash);
        _refreshTokenRepo.ObterPorHashAsync(hash, Arg.Any<CancellationToken>())
            .Returns(tokenExistente);
        _identidade.BuscarPorIdAsync(usuarioId, Arg.Any<CancellationToken>())
            .Returns(usuario);

        var resultado = await CriarHandler().Handle(
            new RefreshTokenCommand("valor"), CancellationToken.None);

        resultado.IsFailed.Should().BeTrue();
        resultado.Errors[0].Message.Should().Be("token_invalido");
    }
}
