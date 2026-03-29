using AutoPartsHub.Application.Common;

namespace AutoPartsHub.Application.Onboarding.Commands;

public record ReenviarVerificacaoCommand(string Email) : ICommand;
