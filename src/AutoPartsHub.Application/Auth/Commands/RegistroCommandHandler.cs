using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using FluentResults;

namespace AutoPartsHub.Application.Auth.Commands;

public sealed class RegistroCommandHandler(
    IIdentidadeService identidade
) : ICommandHandler<RegistroCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegistroCommand command, CancellationToken ct)
    {
        return await identidade.CriarUsuarioAsync(
            command.NomeCompleto,
            command.Email,
            command.Senha,
            command.TenantId,
            command.Role,
            ct);
    }
}
