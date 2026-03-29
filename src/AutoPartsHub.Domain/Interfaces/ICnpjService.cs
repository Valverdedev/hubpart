namespace AutoPartsHub.Domain.Interfaces;

/// <summary>
/// Serviço de consulta de CNPJ na Receita Federal.
/// Retorna null quando a API está indisponível (falha silenciosa).
/// </summary>
public interface ICnpjService
{
    Task<CnpjConsultaResultado?> ConsultarAsync(string cnpj, CancellationToken ct = default);
}
