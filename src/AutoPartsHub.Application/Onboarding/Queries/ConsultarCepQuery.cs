using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;

namespace AutoPartsHub.Application.Onboarding.Queries;

public record ConsultarCepQuery(string Cep) : IQuery<CepInfoDto>;
