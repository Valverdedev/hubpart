using AutoPartsHub.Domain.Interfaces;

namespace AutoPartsHub.Domain.Events;

public sealed record RascunhoAbandonadoEvent(
    string Email,
    Guid SessionToken) : IDomainEvent;
