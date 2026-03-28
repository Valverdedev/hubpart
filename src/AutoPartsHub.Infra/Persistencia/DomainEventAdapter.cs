using AutoPartsHub.Domain.Interfaces;
using MediatR;

namespace AutoPartsHub.Infra.Persistencia;

/// <summary>
/// Adaptador que envolve um IDomainEvent do Domain em um INotification do MediatR.
/// Mantém o Domain sem dependência de MediatR — a ponte fica na camada de Infra.
/// </summary>
internal sealed class DomainEventAdapter(IDomainEvent domainEvent) : INotification
{
    public IDomainEvent DomainEvent { get; } = domainEvent;
}
