using AutoPartsHub.Domain.Interfaces;

namespace AutoPartsHub.Domain.Events;

public sealed record AlertaTrialEvent(
    Guid TenantId,
    string Email,
    string NomeFantasia,
    string Tipo,
    DateTime TrialExpiraEm) : IDomainEvent;
