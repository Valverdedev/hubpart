using AutoPartsHub.Application.Tenants.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AutoPartsHub.API.Endpoints.Tenants;

public static class ConsultarCnpjEndpoint
{
    public static IEndpointRouteBuilder MaparConsultarCnpj(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/tenants/cnpj/{cnpj}", async (
            string cnpj,
            ISender sender,
            CancellationToken ct) =>
        {
            var resultado = await sender.Send(new ConsultarCnpjQuery(cnpj), ct);
            return resultado.ParaIResult();
        })
        .WithName("ConsultarCnpj")
        .WithSummary("Valida o CNPJ e retorna dados pré-preenchidos para o formulário de cadastro")
        .WithTags("Tenants")
        .AllowAnonymous();

        return app;
    }
}
