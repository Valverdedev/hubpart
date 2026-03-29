using AutoPartsHub.Domain.Interfaces;

namespace AutoPartsHub.Domain.Events;

public sealed record TrialExpiradoEvent(
    Guid TenantId,
    string Email,
    string NomeFantasia) : IDomainEvent;
