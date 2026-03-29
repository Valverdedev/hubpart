using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace AutoPartsHub.UnitTests.Auth;

public sealed class RefreshTokenTests
{
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();

    [Fact]
    public void EstaValido_QuandoNaoUsadoNaoRevogadoNaoExpirado_RetornaTrue()
    {
        _dateTime.UtcNow.Returns(DateTime.UtcNow);
        var rt = RefreshToken.Criar("hash123", Guid.NewGuid(), Guid.NewGuid(), _dateTime, diasExpiracao: 7);

        rt.EstaValido(_dateTime).Should().BeTrue();
    }

    [Fact]
    public void EstaValido_QuandoExpirado_RetornaFalse()
    {
        var agora = DateTime.UtcNow;
        _dateTime.UtcNow.Returns(agora);
        var rt = RefreshToken.Criar("hash123", Guid.NewGuid(), Guid.NewGuid(), _dateTime, diasExpiracao: 1);

        // Avança o tempo para após a expiração
        _dateTime.UtcNow.Returns(agora.AddDays(2));

        rt.EstaValido(_dateTime).Should().BeFalse();
    }

    [Fact]
    public void EstaValido_QuandoRevogado_RetornaFalse()
    {
        _dateTime.UtcNow.Returns(DateTime.UtcNow);
        var rt = RefreshToken.Criar("hash123", Guid.NewGuid(), Guid.NewGuid(), _dateTime);
        rt.Revogar(_dateTime);

        rt.EstaValido(_dateTime).Should().BeFalse();
    }

    [Fact]
    public void EstaValido_QuandoUsado_RetornaFalse()
    {
        _dateTime.UtcNow.Returns(DateTime.UtcNow);
        var rt = RefreshToken.Criar("hash123", Guid.NewGuid(), Guid.NewGuid(), _dateTime);
        rt.MarcarComoUsado(_dateTime);

        rt.EstaValido(_dateTime).Should().BeFalse();
    }

    [Fact]
    public void MarcarComoUsado_EmTokenJaUsado_LancaInvalidOperationException()
    {
        _dateTime.UtcNow.Returns(DateTime.UtcNow);
        var rt = RefreshToken.Criar("hash123", Guid.NewGuid(), Guid.NewGuid(), _dateTime);
        rt.MarcarComoUsado(_dateTime);

        var acao = () => rt.MarcarComoUsado(_dateTime);

        acao.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Revogar_EmTokenJaRevogado_LancaInvalidOperationException()
    {
        _dateTime.UtcNow.Returns(DateTime.UtcNow);
        var rt = RefreshToken.Criar("hash123", Guid.NewGuid(), Guid.NewGuid(), _dateTime);
        rt.Revogar(_dateTime);

        var acao = () => rt.Revogar(_dateTime);

        acao.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Criar_ComHashVazio_LancaArgumentException()
    {
        _dateTime.UtcNow.Returns(DateTime.UtcNow);

        var acao = () => RefreshToken.Criar("", Guid.NewGuid(), Guid.NewGuid(), _dateTime);

        acao.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Criar_ComUsuarioIdVazio_LancaArgumentException()
    {
        _dateTime.UtcNow.Returns(DateTime.UtcNow);

        var acao = () => RefreshToken.Criar("hash123", Guid.Empty, Guid.NewGuid(), _dateTime);

        acao.Should().Throw<ArgumentException>();
    }
}
