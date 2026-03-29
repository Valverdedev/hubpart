using AutoPartsHub.Application.Tenants.Commands;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AutoPartsHub.API.Endpoints.Tenants;

public static class CadastrarTenantEndpoint
{
    public static IEndpointRouteBuilder MaparCadastrarTenant(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/tenants", async (
            CadastrarTenantCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var resultado = await sender.Send(command, ct);
            return resultado.ParaIResultCriado(dto => $"/api/v1/tenants/{dto.Id}");
        })
        .WithName("CadastrarTenant")
        .WithSummary("Cadastra um novo tenant (comprador ou fornecedor) na plataforma")
        .WithTags("Tenants")
        .AllowAnonymous();

        return app;
    }
}
