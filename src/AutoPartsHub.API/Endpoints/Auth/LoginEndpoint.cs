using AutoPartsHub.Application.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AutoPartsHub.API.Endpoints.Auth;

public static class LoginEndpoint
{
    /// <summary>
    /// Registra o endpoint POST /api/v1/auth/login.
    /// Chamar em Program.cs: app.MaparLogin()
    /// </summary>
    public static IEndpointRouteBuilder MaparLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/login", async (
            LoginCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var resultado = await sender.Send(command, ct);

            if (resultado.IsSuccess)
                return Results.Ok(resultado.Value);

            // Retorna 422 com a lista de erros de negócio (nunca expõe detalhes internos)
            var erros = resultado.Errors.Select(e => e.Message);
            return Results.UnprocessableEntity(new { erros });
        })
        .WithName("Login")
        .WithSummary("Autentica usuário e retorna JWT + refresh token")
        .WithTags("Auth")
        .AllowAnonymous();

        return app;
    }
}
