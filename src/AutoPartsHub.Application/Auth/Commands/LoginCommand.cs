using AutoPartsHub.Application.Common;

namespace AutoPartsHub.Application.Auth.Commands;

/// <summary>Data returned after successful login.</summary>
public record LoginResultadoDto(
    string Token,
    string RefreshToken,
    DateTime ExpiraEm,
    string NomeCompleto,
    string[] Roles
);

/// <summary>Authentication command: email + password.</summary>
public record LoginCommand(string Email, string Senha) : ICommand<LoginResultadoDto>;
