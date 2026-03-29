using AutoPartsHub.Application.Common;
using AutoPartsHub.Domain.Enums;

namespace AutoPartsHub.Application.Onboarding.Commands;

public record CadastrarCompradorResultadoDto(Guid TenantId, string Status, string RedirectTo = "/login");

public record CadastrarCompradorCommand(
    Guid SessionToken,
    PlanoAssinatura PlanoEscolhido) : ICommand<CadastrarCompradorResultadoDto>;
