using AutoPartsHub.Application.Common;
using AutoPartsHub.Domain.Enums;

namespace AutoPartsHub.Application.Onboarding.Queries;

public record RascunhoDto(TipoComprador TipoPerfil, int UltimoStep, string Dados);

public record ObterRascunhoQuery(Guid SessionToken) : IQuery<RascunhoDto>;
