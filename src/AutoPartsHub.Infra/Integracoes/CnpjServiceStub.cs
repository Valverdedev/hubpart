using AutoPartsHub.Domain.Interfaces;

namespace AutoPartsHub.Infra.Integracoes;

/// <summary>
/// Implementação stub do ICnpjService.
/// Retorna null (API indisponível) até a integração real com a Receita Federal ser implementada.
/// O handler trata null como "prosseguir sem dados pré-preenchidos".
/// </summary>
public sealed class CnpjServiceStub : ICnpjService
{
    public Task<CnpjConsultaResultado?> ConsultarAsync(string cnpj, CancellationToken ct = default)
        => Task.FromResult<CnpjConsultaResultado?>(null);
}
