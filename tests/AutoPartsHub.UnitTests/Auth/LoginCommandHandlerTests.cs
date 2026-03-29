using AutoPartsHub.Application.Auth.Commands;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace AutoPartsHub.UnitTests.Auth;

public sealed class LoginCommandHandlerTests
{
    private readonly IIdentidadeService _identidade = Substitute.For<IIdentidadeService>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IRefreshTokenRepository _refreshTokenRepo = Substitute.For<IRefreshTokenRepository>();
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();

    private LoginCommandHandler CriarHandler() =>
        new(_identidade, _tokenService, _refreshTokenRepo, _dateTime);

    [Fact]
    public async Task Handle_CredenciaisValidas_RetornaLoginResultadoDto()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var dto = new UsuarioDto(usuarioId, tenantId, "user@test.com", "Nome Teste");
        var expiraEm = DateTime.UtcNow.AddHours(1);

        _identidade.BuscarPorEmailAsync("user@test.com", Arg.Any<CancellationToken>())
            .Returns(dto);
        _identidade.ValidarSenhaAsync(usuarioId, "senha123", Arg.Any<CancellationToken>())
            .Returns(true);
        _identidade.ObterRolesAsync(usuarioId, Arg.Any<CancellationToken>())
            .Returns(new List<string> { "Comprador" });
        _tokenService.GerarJwt(usuarioId, tenantId, "user@test.com", "Nome Teste", Arg.Any<IList<string>>())
            .Returns(("jwt_token_aqui", expiraEm));
        _tokenService.GerarRefreshToken().Returns("valor_bruto");
        _tokenService.HashRefreshToken("valor_bruto").Returns("hash_sha256");
        _dateTime.UtcNow.Returns(DateTime.UtcNow);

        // Act
        var resultado = await CriarHandler().Handle(
            new LoginCommand("user@test.com", "senha123"), CancellationToken.None);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Token.Should().Be("jwt_token_aqui");
        resultado.Value.RefreshToken.Should().Be("valor_bruto"); // retorna valor bruto ao cliente
        resultado.Value.NomeCompleto.Should().Be("Nome Teste");
        resultado.Value.Roles.Should().Contain("Comprador");
    }

    [Fact]
    public async Task Handle_UsuarioNaoEncontrado_RetornaFalha()
    {
        _identidade.BuscarPorEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((UsuarioDto?)null);

        var resultado = await CriarHandler().Handle(
            new LoginCommand("nao@existe.com", "qualquer"), CancellationToken.None);

        resultado.IsFailed.Should().BeTrue();
        resultado.Errors[0].Message.Should().Be("credenciais_invalidas");
    }

    [Fact]
    public async Task Handle_SenhaIncorreta_RetornaFalha()
    {
        var dto = new UsuarioDto(Guid.NewGuid(), Guid.NewGuid(), "user@test.com", "Nome");
        _identidade.BuscarPorEmailAsync("user@test.com", Arg.Any<CancellationToken>())
            .Returns(dto);
        _identidade.ValidarSenhaAsync(dto.Id, "errada", Arg.Any<CancellationToken>())
            .Returns(false);

        var resultado = await CriarHandler().Handle(
            new LoginCommand("user@test.com", "errada"), CancellationToken.None);

        resultado.IsFailed.Should().BeTrue();
        resultado.Errors[0].Message.Should().Be("credenciais_invalidas");
    }
}
