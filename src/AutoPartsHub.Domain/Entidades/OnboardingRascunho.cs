using AutoPartsHub.Domain.Enums;
using AutoPartsHub.Domain.Interfaces;

namespace AutoPartsHub.Domain.Entidades;

/// <summary>
/// Entidade de rascunho do fluxo de onboarding.
/// NÃO herda EntidadeBase — não tem tenant_id, não participa do multi-tenancy.
/// Tabela isolada: onboarding_rascunho.
/// </summary>
public sealed class OnboardingRascunho
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>Token enviado ao browser — usado para retomada e finalização.</summary>
    public Guid SessionToken { get; private set; }

    public TipoComprador TipoPerfil { get; private set; }

    /// <summary>Último step concluído — permite retomar do ponto exato de abandono.</summary>
    public int UltimoStep { get; private set; }

    /// <summary>JSON acumulado com todos os campos de cada step.</summary>
    public string Dados { get; private set; } = "{}";

    /// <summary>Email preenchido no step 3 — usado para e-mail de retomada.</summary>
    public string? Email { get; private set; }

    /// <summary>IP de origem — registro para análise de fraude.</summary>
    public string? IpOrigem { get; private set; }

    /// <summary>User-agent — registro para análise de fraude.</summary>
    public string? UserAgent { get; private set; }

    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    // Construtor privado para EF Core
    private OnboardingRascunho() { }

    private OnboardingRascunho(
        TipoComprador tipoPerfil,
        string? ipOrigem,
        string? userAgent,
        IDateTimeProvider dateTime)
    {
        SessionToken = Guid.NewGuid();
        TipoPerfil = tipoPerfil;
        UltimoStep = 1;
        IpOrigem = ipOrigem;
        UserAgent = userAgent;
        CriadoEm = dateTime.UtcNow;
        AtualizadoEm = dateTime.UtcNow;
    }

    /// <summary>Cria um novo rascunho de onboarding.</summary>
    public static OnboardingRascunho Criar(
        TipoComprador tipoPerfil,
        string? ipOrigem,
        string? userAgent,
        IDateTimeProvider dateTime)
        => new(tipoPerfil, ipOrigem, userAgent, dateTime);

    /// <summary>Atualiza os dados parciais e avança o step se necessário.</summary>
    public void Atualizar(int step, string dadosJson, string? email, IDateTimeProvider dateTime)
    {
        Dados = dadosJson;

        // Avança apenas se o step atual for maior que o salvo
        if (step > UltimoStep)
            UltimoStep = step;

        if (email is not null)
            Email = email;

        AtualizadoEm = dateTime.UtcNow;
    }
}
