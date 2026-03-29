using AutoPartsHub.Application.Common;
using AutoPartsHub.Domain.Enums;
using FluentResults;

namespace AutoPartsHub.Application.Onboarding.Commands;

public record IniciarOnboardingCommand(
    TipoComprador TipoPerfil,
    string? IpOrigem,
    string? UserAgent) : ICommand<Guid>;
