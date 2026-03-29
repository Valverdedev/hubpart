using AutoPartsHub.API.Extensions;
using AutoPartsHub.Application.Tenants.Commands;
using AutoPartsHub.Application.Tenants.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoPartsHub.API.Controllers;

/// <summary>Endpoints de gerenciamento de tenants (empresas cadastradas na plataforma).</summary>
[ApiController]
[Route("api/v1/tenants")]
[Produces("application/json")]
public sealed class TenantController(ISender sender) : ControllerBase
{
    /// <summary>Valida o CNPJ e retorna dados pré-preenchidos para o formulário de cadastro.</summary>
    /// <param name="cnpj">CNPJ com 14 dígitos (apenas números).</param>
    /// <response code="200">CNPJ válido — retorna dados da empresa.</response>
    /// <response code="422">CNPJ inválido ou não encontrado.</response>
    [HttpGet("cnpj/{cnpj}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ConsultarCnpj([FromRoute] string cnpj, CancellationToken ct)
    {
        var resultado = await sender.Send(new ConsultarCnpjQuery(cnpj), ct);
        return resultado.ParaActionResult(this);
    }

    /// <summary>Cadastra um novo tenant (comprador ou fornecedor) na plataforma.</summary>
    /// <response code="201">Tenant criado — retorna o Id gerado.</response>
    /// <response code="422">CNPJ já cadastrado ou dados inválidos.</response>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CadastrarTenant([FromBody] CadastrarTenantCommand command, CancellationToken ct)
    {
        var resultado = await sender.Send(command, ct);
        return resultado.ParaActionResultCriado(this, "ObterTenant", dto => new { id = dto.Id });
    }
}
