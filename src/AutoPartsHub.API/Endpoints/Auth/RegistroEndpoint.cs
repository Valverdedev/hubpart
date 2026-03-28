using AutoPartsHub.Application.Auth.Commands;
using AutoPartsHub.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AutoPartsHub.API.Endpoints.Auth;

public static class RegistroEndpoint
{
    /// <summary>
    /// Registra o endpoint POST /api/v1/auth/registro.
    /// Requer autenticação com role Admin.
    /// O TenantId do novo usuário é extraído do JWT do Admin autenticado —
    /// nunca aceito do body, evitando criação em tenants arbitrários.
    /// </summary>
    public static IEndpointRouteBuilder MaparRegistro(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/registro", async (
            RegistroRequest request,
            ISender sender,
            ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            // TenantId vem do JWT do usuário autenticado, não do body
            var command = new RegistroCommand(
                NomeCompleto: request.NomeCompleto,
                Email: request.Email,
                Senha: request.Senha,
                TenantId: tenantContext.TenantId,
                Role: request.Role
            );

            var resultado = await sender.Send(command, ct);
            return resultado.ParaIResultCriado(id => $"/api/v1/usuarios/{id}");
        })
        .WithName("Registro")
        .WithSummary("Registra novo usuário no mesmo tenant do Admin autenticado")
        .WithTags("Auth")
        .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return app;
    }
}

/// <summary>Corpo da requisição de registro — TenantId NÃO é aceito do cliente.</summary>
internal record RegistroRequest(string NomeCompleto, string Email, string Senha, string Role);
