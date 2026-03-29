using Microsoft.OpenApi.Models;

namespace AutoPartsHub.API.Extensions;

public static class SwaggerExtensions
{
    /// <summary>
    /// Registra o Swagger com suporte a JWT Bearer para autenticação interativa no Swagger UI.
    /// </summary>
    public static IServiceCollection AdicionarSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AutoPartsHub API",
                Version = "v1",
                Description = "SaaS B2B de cotação de peças automotivas em tempo real.",
            });

            // Define o esquema de segurança JWT Bearer
            var esquemaJwt = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Informe: Bearer {seu token JWT}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme,
                },
            };

            c.AddSecurityDefinition("Bearer", esquemaJwt);

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { esquemaJwt, Array.Empty<string>() }
            });
        });

        return services;
    }
}
