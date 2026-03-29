using AutoPartsHub.API.Extensions;
using AutoPartsHub.Application.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoPartsHub.API.Controllers;

/// <summary>Endpoints de autenticação: login, refresh de token e registro de usuários.</summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    /// <summary>Autentica o usuário e retorna um par JWT + refresh token.</summary>
    /// <response code="200">Login bem-sucedido — retorna token, refresh token e dados do usuário.</response>
    /// <response code="422">Credenciais inválidas ou validação falhou.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(LoginResultadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var resultado = await sender.Send(command, ct);
        return resultado.ParaActionResult(this);
    }

    /// <summary>Renova o par JWT + refresh token (rotação one-time).</summary>
    /// <response code="200">Token renovado com sucesso.</response>
    /// <response code="422">Token inválido, expirado ou já utilizado.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(LoginResultadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var resultado = await sender.Send(command, ct);
        return resultado.ParaActionResult(this);
    }

    /// <summary>Registra um novo usuário vinculado a um tenant existente. Requer role Admin.</summary>
    /// <response code="201">Usuário criado — retorna o Id gerado.</response>
    /// <response code="422">E-mail já cadastrado ou dados inválidos.</response>
    [HttpPost("registro")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Registro([FromBody] RegistroCommand command, CancellationToken ct)
    {
        var resultado = await sender.Send(command, ct);
        return resultado.ParaActionResultCriado(this, "ObterUsuario", id => new { id });
    }
}
