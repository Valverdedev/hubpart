using AutoPartsHub.Domain.Interfaces;

namespace AutoPartsHub.Domain.Entidades;

public abstract class EntidadeBase
{
    private readonly List<IDomainEvent> _eventos = [];

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; private set; }
    public DateTime? ExcluidoEm { get; private set; }
    public bool ExcluidoLogicamente => ExcluidoEm.HasValue;

    public IReadOnlyCollection<IDomainEvent> Eventos => _eventos.AsReadOnly();

    protected EntidadeBase() { }

    protected EntidadeBase(Guid tenantId)
    {
        TenantId = tenantId;
    }

    protected void DefinirTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId não pode ser vazio.", nameof(tenantId));

        TenantId = tenantId;
    }

    protected void AdicionarEvento(IDomainEvent evento)
        => _eventos.Add(evento);

    public void LimparEventos()
        => _eventos.Clear();

    /// <summary>
    /// Atualiza o timestamp de modificação.
    /// Passe um IDateTimeProvider para controle em testes; sem argumento usa DateTime.UtcNow.
    /// </summary>
    public void MarcarComoAtualizado(IDateTimeProvider? dateTime = null)
    {
        AtualizadoEm = dateTime?.UtcNow ?? DateTime.UtcNow;
    }

    public void ExcluirLogico(IDateTimeProvider? dateTime = null)
    {
        if (ExcluidoLogicamente)
            throw new InvalidOperationException("Entidade já está excluída.");

        var agora = dateTime?.UtcNow ?? DateTime.UtcNow;
        ExcluidoEm = agora;
        AtualizadoEm = agora;
    }

    public void Restaurar(IDateTimeProvider? dateTime = null)
    {
        if (!ExcluidoLogicamente)
            throw new InvalidOperationException("Entidade não está excluída.");

        ExcluidoEm = null;
        AtualizadoEm = dateTime?.UtcNow ?? DateTime.UtcNow;
    }
}
