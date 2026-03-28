namespace AutoPartsHub.Domain.Interfaces;

/// <summary>
/// Marca um evento de domínio. O Domain não depende de MediatR —
/// a conversão para INotification é feita na camada de Infra ao publicar.
/// </summary>
public interface IDomainEvent;
