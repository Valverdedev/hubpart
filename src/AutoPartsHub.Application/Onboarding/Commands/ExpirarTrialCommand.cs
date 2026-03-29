using AutoPartsHub.Application.Common;

namespace AutoPartsHub.Application.Onboarding.Commands;

public record ExpirarTrialResultadoDto(int Expirados, int AlertasD7, int AlertasD1);

public record ExpirarTrialCommand : ICommand<ExpirarTrialResultadoDto>;
