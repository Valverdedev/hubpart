using AutoPartsHub.Application.Common;

namespace AutoPartsHub.Application.Auth.Commands;

/// <summary>
/// Rotates refresh token and issues a new JWT pair.
/// </summary>
public record RefreshTokenCommand(string RefreshToken) : ICommand<LoginResultadoDto>;
