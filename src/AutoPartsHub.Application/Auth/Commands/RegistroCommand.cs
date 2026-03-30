using AutoPartsHub.Application.Common;

namespace AutoPartsHub.Application.Auth.Commands;

/// <summary>
/// Registers a new user linked to a tenant.
/// TenantId is injected by controller from JWT.
/// </summary>
public record RegistroCommand(
    string NomeCompleto,
    string Email,
    string Senha,
    Guid TenantId,
    string Role
) : ICommand<Guid>;

/// <summary>Request body payload. TenantId is not exposed in HTTP body.</summary>
public record RegistroInput(
    string NomeCompleto,
    string Email,
    string Senha,
    string Role
);
