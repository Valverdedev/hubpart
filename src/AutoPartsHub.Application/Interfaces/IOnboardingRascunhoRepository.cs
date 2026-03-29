using AutoPartsHub.Domain.Entidades;

namespace AutoPartsHub.Application.Interfaces;

public interface IOnboardingRascunhoRepository
{
    Task<OnboardingRascunho?> BuscarPorTokenAsync(Guid sessionToken, CancellationToken ct);
    Task AdicionarAsync(OnboardingRascunho rascunho, CancellationToken ct);
    Task AtualizarAsync(OnboardingRascunho rascunho, CancellationToken ct);
    Task DeletarAsync(OnboardingRascunho rascunho, CancellationToken ct);
    Task<List<OnboardingRascunho>> BuscarAntigosAsync(DateTime antes, CancellationToken ct);
}
