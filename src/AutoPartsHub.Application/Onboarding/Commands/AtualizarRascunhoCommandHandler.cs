using System.Text.Json;
using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Interfaces;
using FluentResults;

namespace AutoPartsHub.Application.Onboarding.Commands;

public sealed class AtualizarRascunhoCommandHandler(
    IOnboardingRascunhoRepository repositorio,
    IDateTimeProvider dateTime) : ICommandHandler<AtualizarRascunhoCommand>
{
    public async Task<Result> Handle(AtualizarRascunhoCommand request, CancellationToken cancellationToken)
    {
        var rascunho = await repositorio.BuscarPorTokenAsync(request.SessionToken, cancellationToken);

        if (rascunho is null)
            return Result.Fail("rascunho_nao_encontrado");

        var dadosJson = JsonSerializer.Serialize(request.Dados);

        rascunho.Atualizar(request.Step, dadosJson, request.Email, dateTime);

        await repositorio.AtualizarAsync(rascunho, cancellationToken);

        return Result.Ok();
    }
}
