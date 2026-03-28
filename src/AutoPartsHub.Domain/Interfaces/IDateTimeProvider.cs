namespace AutoPartsHub.Domain.Interfaces;

/// <summary>
/// Abstração do relógio do sistema. Substitui DateTime.UtcNow direto no código,
/// permitindo controle total do tempo em testes unitários.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
