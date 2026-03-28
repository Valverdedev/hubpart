using AutoPartsHub.Application.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AutoPartsHub.API.Endpoints.Auth;

public static class RefreshTokenEndpoint
{
    /// <summary>
    /// Registra o endpoint POST /api/v1/auth/refresh.
    /// Chamar em Program.cs: app.MaparRefreshToken()
    /// </summary>
    public static IEndpointRouteBuilder MaparRefreshToken(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/refresh", async (
            RefreshTokenRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new RefreshTokenCommand(request.RefreshToken);
            var resultado = await sender.Send(command, ct);
            return resultado.ParaIResult();
        })
        .WithName("RefreshToken")
        .WithSummary("Renova JWT usando refresh token (rotação — token antigo é invalidado)")
        .WithTags("Auth")
        .AllowAnonymous();

        return app;
    }
}

/// <summary>Corpo da requisição de renovação de token.</summary>
internal record RefreshTokenRequest(string RefreshToken);
