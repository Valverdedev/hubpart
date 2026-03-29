using System.Net.Http.Json;
using AutoPartsHub.Application.Auth.Commands;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AutoPartsHub.IntegrationTests.Auth;

/// <summary>
/// Testa o fluxo completo de autenticação contra uma instância real da API.
/// Requer banco PostgreSQL disponível (configurado via appsettings.Test.json ou variável de ambiente).
/// </summary>
public sealed class AuthFluxoTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _cliente = factory.CreateClient();

    [Fact]
    public async Task Login_CredenciaisValidas_RetornaTokenERefreshToken()
    {
        var command = new LoginCommand("admin@autopartshub.com", "Admin@123");

        var resposta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", command);

        resposta.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var resultado = await resposta.Content.ReadFromJsonAsync<LoginResultadoDto>();
        resultado.Should().NotBeNull();
        resultado!.Token.Should().NotBeNullOrWhiteSpace();
        resultado.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_TokenValido_RotacionaERetornaNovoToken()
    {
        // 1. Login
        var loginCmd = new LoginCommand("admin@autopartshub.com", "Admin@123");
        var loginResp = await _cliente.PostAsJsonAsync("/api/v1/auth/login", loginCmd);
        loginResp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var loginDto = await loginResp.Content.ReadFromJsonAsync<LoginResultadoDto>();

        // 2. Refresh
        var refreshCmd = new RefreshTokenCommand(loginDto!.RefreshToken);
        var refreshResp = await _cliente.PostAsJsonAsync("/api/v1/auth/refresh", refreshCmd);

        refreshResp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var refreshDto = await refreshResp.Content.ReadFromJsonAsync<LoginResultadoDto>();
        refreshDto!.Token.Should().NotBe(loginDto.Token);
        refreshDto.RefreshToken.Should().NotBe(loginDto.RefreshToken);
    }

    [Fact]
    public async Task Refresh_TokenAntigo_RejetadoAposRotacao()
    {
        // 1. Login
        var loginCmd = new LoginCommand("admin@autopartshub.com", "Admin@123");
        var loginResp = await _cliente.PostAsJsonAsync("/api/v1/auth/login", loginCmd);
        var loginDto = await loginResp.Content.ReadFromJsonAsync<LoginResultadoDto>();

        // 2. Primeiro refresh — consome o token original
        await _cliente.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshTokenCommand(loginDto!.RefreshToken));

        // 3. Tenta usar o token original novamente — deve ser rejeitado
        var reuso = await _cliente.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshTokenCommand(loginDto.RefreshToken));

        reuso.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_CredenciaisInvalidas_Retorna422()
    {
        var command = new LoginCommand("naoexiste@test.com", "senhaerrada1");

        var resposta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", command);

        resposta.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
    }
}
