using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace AutoPartsHub.API.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AdicionarOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((documento, contexto, ct) =>
            {
                documento.Info = new OpenApiInfo
                {
                    Title       = "AutoPartsHub API",
                    Version     = "v1",
                    Description = "SaaS B2B de cotação de peças automotivas em tempo real.\n\n" +
                                  "Conecta oficinas, frotas e revendas a fornecedores locais via cotação em tempo real.\n\n" +
                                  "**Autenticação:** Bearer JWT — obtenha o token em `POST /api/v1/auth/login`.",
                    Contact = new OpenApiContact
                    {
                        Name  = "AutoPartsHub",
                        Email = "dev@autopartshub.com.br"
                    }
                };

                return Task.CompletedTask;
            });

            // Esquema JWT — aplica o cadeado nos endpoints com RequireAuthorization
            options.AddDocumentTransformer<JwtBearerSecuritySchemeTransformer>();
        });

        return services;
    }

    public static WebApplication UsarScalar(this WebApplication app)
    {
        app.MapOpenApi();

        app.MapScalarApiReference(scalar =>
        {
            scalar
                .WithTitle("AutoPartsHub API")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithPreferredScheme(JwtBearerDefaults.AuthenticationScheme);
        });

        return app;
    }
}

/// <summary>
/// Registra o esquema BearerAuth no documento OpenAPI quando JWT está configurado.
/// </summary>
internal sealed class JwtBearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider schemeProvider
) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument documento,
        OpenApiDocumentTransformerContext contexto,
        CancellationToken ct)
    {
        var esquemas = await schemeProvider.GetAllSchemesAsync();
        if (!esquemas.Any(s => s.Name == JwtBearerDefaults.AuthenticationScheme))
            return;

        var securityScheme = new OpenApiSecurityScheme
        {
            Type         = SecuritySchemeType.Http,
            Scheme       = "bearer",
            BearerFormat = "JWT",
            In           = ParameterLocation.Header,
            Name         = "Authorization",
            Description  = "Informe o token JWT obtido em `POST /api/v1/auth/login`.\n\nExemplo: `Bearer eyJhbGci...`"
        };

        documento.Components ??= new OpenApiComponents();
        documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        documento.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = securityScheme;
    }
}
