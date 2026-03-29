using AutoPartsHub.Domain.Enums;
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
        Endereco endereco)
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
            DateTime.UtcNow.AddDays(30),
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
        Endereco endereco)
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
            DateTime.UtcNow.AddDays(30),
            endereco);
    }

    /// <summary>Define ou atualiza as coordenadas geográficas do fornecedor.</summary>
    public void DefinirLocalizacao(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}
