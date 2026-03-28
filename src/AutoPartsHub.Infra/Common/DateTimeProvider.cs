using AutoPartsHub.Domain.Interfaces;

namespace AutoPartsHub.Infra.Common;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
