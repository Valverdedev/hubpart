using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using FluentResults;
using FluentValidation;

namespace AutoPartsHub.Application.Auth.Commands;

// ---------------------------------------------------------------------------
// Command
// ---------------------------------------------------------------------------

/// <summary>
/// Registra um novo usuário vinculado a um tenant.
/// TenantId é obrigatório, mas nunca vem do body HTTP — o controller o injeta a partir do
/// JWT do Admin autenticado, evitando criação cross-tenant.
/// </summary>
public record RegistroCommand(
    string NomeCompleto,
    string Email,
    string Senha,
    Guid TenantId,  // preenchido pelo controller via ITenantContext — não expor no body
    string Role
) : ICommand<Guid>;

/// <summary>Payload recebido do body. Sem TenantId — vem sempre do JWT.</summary>
public record RegistroInput(
    string NomeCompleto,
    string Email,
    string Senha,
    string Role
);

// ---------------------------------------------------------------------------
// Validator
// ---------------------------------------------------------------------------

public sealed class RegistroCommandValidator : AbstractValidator<RegistroCommand>
{
    public RegistroCommandValidator()
    {
        RuleFor(c => c.NomeCompleto)
            .NotEmpty().WithMessage("nome_obrigatorio")
            .MaximumLength(200).WithMessage("nome_maximo_200_caracteres");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("email_obrigatorio")
            .EmailAddress().WithMessage("email_invalido");

        // Política alinhada com IdentityExtensions: mínimo 8, 1 dígito, 1 minúscula
        RuleFor(c => c.Senha)
            .NotEmpty().WithMessage("senha_obrigatoria")
            .MinimumLength(8).WithMessage("senha_minimo_8_caracteres")
            .Matches(@"[a-z]").WithMessage("senha_requer_minuscula")
            .Matches(@"[0-9]").WithMessage("senha_requer_numero");

        // TenantId é injetado pelo controller (JWT) — validado internamente no handler
        RuleFor(c => c.TenantId)
            .NotEmpty().WithMessage("tenant_obrigatorio");

        RuleFor(c => c.Role)
            .NotEmpty().WithMessage("role_obrigatoria")
            .Must(r => new[] { "Admin", "Comprador", "Fornecedor", "Aprovador" }.Contains(r))
            .WithMessage("role_invalida");
    }
}

// ---------------------------------------------------------------------------
// Handler
// ---------------------------------------------------------------------------

public sealed class RegistroCommandHandler(
    IIdentidadeService identidade
) : ICommandHandler<RegistroCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegistroCommand command, CancellationToken ct)
    {
        return await identidade.CriarUsuarioAsync(
            command.NomeCompleto,
            command.Email,
            command.Senha,
            command.TenantId,
            command.Role,
            ct);
    }
}
