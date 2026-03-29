using System.Text;
using AutoPartsHub.Infra.Identity;
using AutoPartsHub.Infra.Persistencia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AutoPartsHub.API.Extensions;

public static class IdentityExtensions
{
    /// <summary>
    /// Configura ASP.NET Identity, JWT Bearer e o contexto de tenant.
    /// Chamar em Program.cs: builder.Services.AdicionarIdentity(builder.Configuration)
    /// </summary>
    public static IServiceCollection AdicionarIdentity(
        this IServiceCollection services,
        IConfiguration configuracao)
    {
        // --- ASP.NET Identity ---
        services
            .AddIdentityCore<UsuarioApp>(opcoes =>
            {
                // Política de senhas — pode ser ajustada por plano no futuro
                opcoes.Password.RequiredLength = 8;
                opcoes.Password.RequireDigit = true;
                opcoes.Password.RequireLowercase = true;
                opcoes.Password.RequireUppercase = false;
                opcoes.Password.RequireNonAlphanumeric = false;

                // Usuário único por e-mail
                opcoes.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // --- JWT Bearer ---
        var segredo = configuracao["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret não configurado em appsettings.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opcoes =>
            {
                opcoes.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuracao["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = configuracao["Jwt:Audience"],

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(segredo)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero, // sem margem extra de expiração
                };

                // Suporte a JWT em conexões SignalR (token na query string)
                opcoes.Events = new JwtBearerEvents
                {
                    OnMessageReceived = contexto =>
                    {
                        var tokenQuery = contexto.Request.Query["access_token"];
                        var caminho = contexto.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(tokenQuery) && caminho.StartsWithSegments("/hubs"))
                            contexto.Token = tokenQuery;

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        // IHttpContextAccessor e ITenantContext são registrados em Program.cs
        // antes do AddDbContext — não registrar aqui para evitar duplo registro.

        return services;
    }
}
