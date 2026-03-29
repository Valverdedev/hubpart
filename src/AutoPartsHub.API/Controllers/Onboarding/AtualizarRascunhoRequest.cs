namespace AutoPartsHub.API.Controllers.Onboarding;

public sealed record AtualizarRascunhoRequest(
    int Step,
    Dictionary<string, object> Dados,
    string? Email);
