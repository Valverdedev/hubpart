using AutoPartsHub.Domain.Enums;
using AutoPartsHub.Domain.Interfaces;

namespace AutoPartsHub.Domain.Events;

public sealed record CompradorCadastradoEvent(
    Guid TenantId,
    string Email,
    string NomeFantasia,
    PlanoAssinatura PlanoAtual,
    DateTime? TrialExpiraEm) : IDomainEvent;
