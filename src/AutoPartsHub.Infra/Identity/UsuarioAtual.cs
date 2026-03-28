using System.Security.Claims;
using AutoPartsHub.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AutoPartsHub.Infra.Identity;

/// <summary>
/// Implementação de IUsuarioAtual que extrai dados do usuário autenticado do JWT.
/// Registrado como Scoped — cada request recebe sua própria instância.
/// </summary>
public sealed class UsuarioAtual : IUsuarioAtual
{
    private readonly Lazy<Guid> _id;
    private readonly Lazy<string?> _email;
    private readonly Lazy<bool> _autenticado;

    public UsuarioAtual(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;

        _autenticado = new Lazy<bool>(() => user?.Identity?.IsAuthenticated is true);
        _id = new Lazy<Guid>(() =>
        {
            var valor = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user?.FindFirst("sub")?.Value;
            return Guid.TryParse(valor, out var id) ? id : Guid.Empty;
        });
        _email = new Lazy<string?>(() => user?.FindFirst(ClaimTypes.Email)?.Value
                                      ?? user?.FindFirst("email")?.Value);
    }

    public Guid Id => _id.Value;
    public string? Email => _email.Value;
    public bool Autenticado => _autenticado.Value;
}
