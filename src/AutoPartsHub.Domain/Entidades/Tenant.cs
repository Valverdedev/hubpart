using AutoPartsHub.Domain.Enums;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Domain.ValueObjects;

namespace AutoPartsHub.Domain.Entidades;

/// <summary>
/// Aggregate raiz que representa uma empresa cadastrada na plataforma.
/// O TenantId é igual ao próprio Id — este aggregate é a raiz do tenant.
/// </summary>
public sealed class Tenant : EntidadeBase
{
    public string RazaoSocial { get; private set; } = string.Empty;
    public string NomeFantasia { get; private set; } = string.Empty;
    public Cnpj Cnpj { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public TipoTenant Tipo { get; private set; }
    public StatusTenant Status { get; private set; }
    public PlanoTenant Plano { get; private set; }
    public DateTime TrialExpiraEm { get; private set; }
    public DateTime? AssinaturaRenovaEm { get; private set; }
    public int CotacoesUsadasNoCiclo { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public Endereco Endereco { get; private set; } = null!;

    // --- Campos de onboarding do comprador ---

    /// <summary>Tipo detalhado do comprador — OficinaCarro, OficinaMoto, Logista, Frotista, Outro.</summary>
    public TipoComprador? TipoComprador { get; private set; }

    /// <summary>Plano de assinatura atual.</summary>
    public PlanoAssinatura PlanoAtual { get; private set; }

    /// <summary>Status da assinatura: Trial, Free, Ativo, Bloqueado.</summary>
    public StatusAssinatura StatusAssinatura { get; private set; }

    /// <summary>Data de expiração do trial. Null para tipo Outro.</summary>
    public DateTime? TrialExpiraEmNovo { get; private set; }

    /// <summary>Quantidade máxima de cotações por mês — desnormalizado do plano para consulta rápida.</summary>
    public int CotacoesLimiteMes { get; private set; }

    /// <summary>Quantidade máxima de usuários — desnormalizado do plano.</summary>
    public int UsuariosLimite { get; private set; }

    public string? InscricaoEstadual { get; private set; }
    public string TelefoneComercial { get; private set; } = string.Empty;
    public string? ComoNosConheceu { get; private set; }

    /// <summary>Descrição livre — somente tipo Outro.</summary>
    public string? DescricaoOutro { get; private set; }

    /// <summary>Segmento da frota — somente Frotista.</summary>
    public string? SegmentoFrota { get; private set; }

    /// <summary>Quantidade estimada de veículos — somente Frotista.</summary>
    public int? QtdVeiculosEstimada { get; private set; }

    /// <summary>Limite de aprovação individual — somente Frotista.</summary>
    public decimal? LimiteAprovacaoAdmin { get; private set; }

    private readonly List<Telefone> _telefones = [];
    public IReadOnlyCollection<Telefone> Telefones => _telefones.AsReadOnly();

    // Construtor privado para EF Core (instanciação via proxy)
    private Tenant() { }

    private Tenant(
        string razaoSocial,
        string nomeFantasia,
        Cnpj cnpj,
        Email email,
        IEnumerable<Telefone> telefones,
        TipoTenant tipo,
        StatusTenant status,
        PlanoTenant plano,
        DateTime trialExpiraEm,
        Endereco endereco)
    {
        RazaoSocial = razaoSocial;
        NomeFantasia = nomeFantasia;
        Cnpj = cnpj;
        Email = email;
        _telefones.AddRange(telefones);
        Tipo = tipo;
        Status = status;
        Plano = plano;
        TrialExpiraEm = trialExpiraEm;
        Endereco = endereco;

        // O próprio Tenant é a raiz do tenant — TenantId == Id
        DefinirTenant(Id);
    }

    /// <summary>
    /// Cria um tenant do tipo comprador (Oficina, Frota ou Revenda).
    /// Entra diretamente como Ativo no plano Free com trial de 30 dias.
    /// </summary>
    public static Tenant CriarComprador(
        string razaoSocial,
        string nomeFantasia,
        Cnpj cnpj,
        Email email,
        IEnumerable<Telefone> telefones,
        TipoTenant tipo,
        Endereco endereco,
        IDateTimeProvider dateTime)
    {
        return new Tenant(
            razaoSocial,
            nomeFantasia,
            cnpj,
            email,
            telefones,
            tipo,
            StatusTenant.Ativo,
            PlanoTenant.Free,
            dateTime.UtcNow.AddDays(30),
            endereco);
    }

    /// <summary>
    /// Cria um tenant do tipo Fornecedor.
    /// Entra como AguardandoAprovacao — sem acesso até admin aprovar.
    /// Trial de 30 dias já começa no cadastro.
    /// </summary>
    public static Tenant CriarFornecedor(
        string razaoSocial,
        string nomeFantasia,
        Cnpj cnpj,
        Email email,
        IEnumerable<Telefone> telefones,
        Endereco endereco,
        IDateTimeProvider dateTime)
    {
        return new Tenant(
            razaoSocial,
            nomeFantasia,
            cnpj,
            email,
            telefones,
            TipoTenant.Fornecedor,
            StatusTenant.AguardandoAprovacao,
            PlanoTenant.Free,
            dateTime.UtcNow.AddDays(30),
            endereco);
    }

    /// <summary>
    /// Cria um tenant via fluxo de onboarding do comprador.
    /// Define plano, limites e status baseado no tipo e plano escolhido.
    /// CNPJ deve conter exatamente 14 dígitos numéricos (sem máscara).
    /// </summary>
    public static Tenant Criar(
        string nomeFantasia,
        string razaoSocial,
        string cnpjNumerico,
        string emailAdmin,
        TipoComprador tipoComprador,
        PlanoAssinatura planoEscolhido,
        string telefoneComercial,
        string? inscricaoEstadual,
        string? comoNosConheceu,
        string? descricaoOutro,
        string? segmentoFrota,
        int? qtdVeiculosEstimada,
        decimal? limiteAprovacaoAdmin,
        EnderecoOnboarding endereco,
        IDateTimeProvider dateTime)
    {
        // Valida CNPJ: deve ter 14 dígitos numéricos
        if (string.IsNullOrWhiteSpace(cnpjNumerico) || cnpjNumerico.Length != 14 || !cnpjNumerico.All(char.IsDigit))
            throw new ArgumentException("CNPJ deve conter exatamente 14 dígitos numéricos.", nameof(cnpjNumerico));

        var cnpjCriado = Cnpj.Criar(cnpjNumerico);
        if (cnpjCriado.IsFailed)
            throw new ArgumentException(cnpjCriado.Errors[0].Message, nameof(cnpjNumerico));

        var emailCriado = Email.Criar(emailAdmin);
        if (emailCriado.IsFailed)
            throw new ArgumentException(emailCriado.Errors[0].Message, nameof(emailAdmin));

        // Tipo Outro → sempre Free, sem trial
        PlanoAssinatura planoFinal;
        StatusAssinatura statusFinal;
        DateTime? trialExpiraFinal;

        if (tipoComprador == Enums.TipoComprador.Outro)
        {
            planoFinal = PlanoAssinatura.Free;
            statusFinal = StatusAssinatura.Free;
            trialExpiraFinal = null;
        }
        else
        {
            planoFinal = planoEscolhido;
            statusFinal = StatusAssinatura.Trial;
            trialExpiraFinal = dateTime.UtcNow.AddDays(30);
        }

        // Limites desnormalizados do plano
        var (cotacoes, usuarios) = ObterLimitesPlano(planoFinal);

        var enderecoVo = EnderecoSimples.Criar(
            endereco.Cep,
            endereco.Logradouro,
            endereco.Numero,
            endereco.Complemento,
            endereco.Bairro,
            endereco.Cidade,
            endereco.Estado);

        var tenant = new Tenant
        {
            RazaoSocial = razaoSocial,
            NomeFantasia = nomeFantasia,
            Cnpj = cnpjCriado.Value,
            Email = emailCriado.Value,
            Tipo = MapearTipoTenant(tipoComprador),
            Status = StatusTenant.Ativo,
            Plano = PlanoTenant.Free,
            TrialExpiraEm = trialExpiraFinal ?? dateTime.UtcNow.AddDays(30),
            TipoComprador = tipoComprador,
            PlanoAtual = planoFinal,
            StatusAssinatura = statusFinal,
            TrialExpiraEmNovo = trialExpiraFinal,
            CotacoesLimiteMes = cotacoes,
            UsuariosLimite = usuarios,
            TelefoneComercial = telefoneComercial,
            InscricaoEstadual = inscricaoEstadual,
            ComoNosConheceu = comoNosConheceu,
            DescricaoOutro = tipoComprador == Enums.TipoComprador.Outro ? descricaoOutro : null,
            SegmentoFrota = tipoComprador == Enums.TipoComprador.Frotista ? segmentoFrota : null,
            QtdVeiculosEstimada = tipoComprador == Enums.TipoComprador.Frotista ? qtdVeiculosEstimada : null,
            LimiteAprovacaoAdmin = tipoComprador == Enums.TipoComprador.Frotista ? limiteAprovacaoAdmin : null,
            EnderecoSimples = enderecoVo,
            Endereco = Endereco.Criar(
                endereco.Cep,
                endereco.Logradouro,
                endereco.Numero,
                endereco.Complemento,
                endereco.Bairro,
                0,
                0),
            // Campos herdados de EntidadeBase são inicializados automaticamente
        };

        // O Tenant é a raiz — TenantId == Id
        tenant.DefinirTenant(tenant.Id);

        return tenant;
    }

    /// <summary>Retorna (cotacoesLimiteMes, usuariosLimite) para o plano informado.</summary>
    private static (int cotacoes, int usuarios) ObterLimitesPlano(PlanoAssinatura plano) => plano switch
    {
        PlanoAssinatura.Free => (10, 1),
        PlanoAssinatura.Basico => (50, 2),
        PlanoAssinatura.Profissional => (200, 5),
        PlanoAssinatura.Enterprise => (int.MaxValue, int.MaxValue),
        _ => (10, 1)
    };

    private static TipoTenant MapearTipoTenant(TipoComprador tipoComprador) => tipoComprador switch
    {
        Enums.TipoComprador.OficinaCarro => TipoTenant.Oficina,
        Enums.TipoComprador.OficinaMoto => TipoTenant.Oficina,
        Enums.TipoComprador.Frotista => TipoTenant.Frota,
        Enums.TipoComprador.Logista => TipoTenant.Revenda,
        Enums.TipoComprador.Outro => TipoTenant.Revenda,
        _ => TipoTenant.Oficina
    };

    /// <summary>
    /// Rebaixa o tenant para Free após expiração do trial.
    /// Chamado pelo ExpirarTrialCommandHandler.
    /// </summary>
    public void RebaixarParaFree(IDateTimeProvider dateTime)
    {
        StatusAssinatura = StatusAssinatura.Free;
        PlanoAtual = PlanoAssinatura.Free;
        CotacoesLimiteMes = 10;
        UsuariosLimite = 1;
        MarcarComoAtualizado(dateTime);
    }

    public EnderecoSimples? EnderecoSimples { get; private set; }

    /// <summary>Define ou atualiza as coordenadas geográficas do fornecedor.</summary>
    public void DefinirLocalizacao(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}
