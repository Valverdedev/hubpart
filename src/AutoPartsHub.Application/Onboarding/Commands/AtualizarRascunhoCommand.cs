using AutoPartsHub.Application.Common;

namespace AutoPartsHub.Application.Onboarding.Commands;

public record AtualizarRascunhoCommand(
    Guid SessionToken,
    int Step,
    Dictionary<string, object> Dados,
    string? Email) : ICommand;
