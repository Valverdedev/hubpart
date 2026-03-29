using System.Text.Json;
using AutoPartsHub.API.Controllers.Onboarding;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Application.Onboarding.Commands;
using AutoPartsHub.Application.Onboarding.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoPartsHub.API.Controllers;

[ApiController]
[Route("api/v1/onboarding")]
[Produces("application/json")]
public sealed class OnboardingController(ISender sender) : ControllerBase
{
    [HttpGet("cnpj/{cnpj}")]
    [EnableRateLimiting("onboarding-cnpj")]
    [ProducesResponseType(typeof(CnpjInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarCnpj([FromRoute] string cnpj, CancellationToken ct)
    {
        var resultado = await sender.Send(new ConsultarCnpjOnboardingQuery(cnpj), ct);

        if (resultado.IsSuccess)
            return Ok(resultado.Value);

        return NotFound();
    }

    [HttpGet("cep/{cep}")]
    [EnableRateLimiting("onboarding-cep")]
    [ProducesResponseType(typeof(CepInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarCep([FromRoute] string cep, CancellationToken ct)
    {
        var resultado = await sender.Send(new ConsultarCepQuery(cep), ct);

        if (resultado.IsSuccess)
            return Ok(resultado.Value);

        return NotFound();
    }

    [HttpPost("iniciar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Iniciar([FromBody] IniciarOnboardingRequest request, CancellationToken ct)
    {
        var command = new IniciarOnboardingCommand(request.TipoPerfil, request.IpOrigem, request.UserAgent);
        var resultado = await sender.Send(command, ct);

        if (resultado.IsFailed)
            return UnprocessableEntity(new { erros = resultado.Errors.Select(e => e.Message) });

        return Created(string.Empty, new { sessionToken = resultado.Value });
    }

    [HttpPut("rascunho/{sessionToken:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AtualizarRascunho(
        [FromRoute] Guid sessionToken,
        [FromBody] AtualizarRascunhoRequest request,
        CancellationToken ct)
    {
        var command = new AtualizarRascunhoCommand(
            sessionToken,
            request.Step,
            request.Dados,
            request.Email);

        var resultado = await sender.Send(command, ct);

        if (resultado.IsSuccess)
            return Ok();

        if (resultado.Errors.Any(e => e.Message == "rascunho_nao_encontrado"))
            return NotFound();

        return UnprocessableEntity(new { erros = resultado.Errors.Select(e => e.Message) });
    }

    [HttpGet("rascunho/{sessionToken:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterRascunho([FromRoute] Guid sessionToken, CancellationToken ct)
    {
        var resultado = await sender.Send(new ObterRascunhoQuery(sessionToken), ct);

        if (resultado.IsFailed)
            return NotFound();

        object? dados;
        try
        {
            dados = JsonSerializer.Deserialize<object>(resultado.Value.Dados);
        }
        catch
        {
            dados = null;
        }

        return Ok(new
        {
            tipoPerfil = resultado.Value.TipoPerfil,
            ultimoStep = resultado.Value.UltimoStep,
            dados
        });
    }

    [HttpPost("finalizar/{sessionToken:guid}")]
    [ProducesResponseType(typeof(CadastrarCompradorResultadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Finalizar(
        [FromRoute] Guid sessionToken,
        [FromBody] FinalizarOnboardingRequest request,
        CancellationToken ct)
    {
        var resultado = await sender.Send(new CadastrarCompradorCommand(sessionToken, request.PlanoEscolhido), ct);

        if (resultado.IsSuccess)
            return Ok(resultado.Value);

        if (resultado.Errors.Any(e => e.Message == "sessao_expirada"))
        {
            return BadRequest(new
            {
                erro = "sessao_expirada",
                mensagem = "Seu rascunho expirou. Inicie um novo cadastro."
            });
        }

        return UnprocessableEntity(new { erros = resultado.Errors.Select(e => e.Message) });
    }

    [HttpPost("reenviar-verificacao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReenviarVerificacao([FromBody] ReenviarVerificacaoRequest request, CancellationToken ct)
    {
        await sender.Send(new ReenviarVerificacaoCommand(request.Email), ct);
        return Ok();
    }
}
