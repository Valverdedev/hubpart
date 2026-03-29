using AutoPartsHub.Application.Common;

namespace AutoPartsHub.Application.Tenants.Queries;

public record ConsultarCnpjQuery(string Cnpj) : IQuery<ConsultarCnpjResultadoDto>;
