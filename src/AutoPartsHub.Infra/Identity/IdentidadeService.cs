using AutoPartsHub.Application.Interfaces;
using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace AutoPartsHub.Infra.Identity;

/// <summary>
/// Implementação de IIdentidadeService usando UserManager&lt;UsuarioApp&gt; do ASP.NET Identity.
/// Encapsula toda a lógica de Identity para que a camada Application não referencie frameworks
/// de infraestrutura diretamente.
/// </summary>
public sealed class IdentidadeService(UserManager<UsuarioApp> userManager) : IIdentidadeService
{
    public async Task<UsuarioDto?> BuscarPorEmailAsync(string email, CancellationToken ct = default)
    {
        var usuario = await userManager.FindByEmailAsync(email);
        return usuario is null ? null : ToDto(usuario);
    }

    public async Task<UsuarioDto?> BuscarPorIdAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var usuario = await userManager.FindByIdAsync(usuarioId.ToString());
        return usuario is null ? null : ToDto(usuario);
    }

    public async Task<bool> ValidarSenhaAsync(Guid usuarioId, string senha, CancellationToken ct = default)
    {
        var usuario = await userManager.FindByIdAsync(usuarioId.ToString());
        if (usuario is null) return false;
        return await userManager.CheckPasswordAsync(usuario, senha);
    }

    public async Task<IList<string>> ObterRolesAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var usuario = await userManager.FindByIdAsync(usuarioId.ToString());
        if (usuario is null) return Array.Empty<string>();
        return await userManager.GetRolesAsync(usuario);
    }

    public async Task AtualizarUltimoLoginAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var usuario = await userManager.FindByIdAsync(usuarioId.ToString());
        if (usuario is null) return;

        usuario.UltimoLoginEm = DateTime.UtcNow;
        await userManager.UpdateAsync(usuario);
    }

    public async Task<Result<Guid>> CriarUsuarioAsync(
        string nomeCompleto,
        string email,
        string senha,
        Guid tenantId,
        string role,
        CancellationToken ct = default)
    {
        var existente = await userManager.FindByEmailAsync(email);
        if (existente is not null)
            return Result.Fail<Guid>("email_ja_cadastrado");

        var usuario = new UsuarioApp
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            NomeCompleto = nomeCompleto,
            Email = email,
            UserName = email,
        };

        var resultado = await userManager.CreateAsync(usuario, senha);
        if (!resultado.Succeeded)
        {
            var erros = resultado.Errors.Select(e => e.Description);
            return Result.Fail<Guid>(string.Join(" | ", erros));
        }

        var resultadoRole = await userManager.AddToRoleAsync(usuario, role);
        if (!resultadoRole.Succeeded)
        {
            var erros = resultadoRole.Errors.Select(e => e.Description);
            return Result.Fail<Guid>(string.Join(" | ", erros));
        }

        return Result.Ok(usuario.Id);
    }

    private static UsuarioDto ToDto(UsuarioApp usuario)
        => new(usuario.Id, usuario.TenantId, usuario.Email!, usuario.NomeCompleto);
}
