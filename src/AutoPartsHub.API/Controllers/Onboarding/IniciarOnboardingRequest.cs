using AutoPartsHub.Domain.Enums;

namespace AutoPartsHub.API.Controllers.Onboarding;

public sealed record IniciarOnboardingRequest(
    TipoComprador TipoPerfil,
    string? IpOrigem,
    string? UserAgent);
