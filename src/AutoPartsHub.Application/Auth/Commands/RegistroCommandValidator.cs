using FluentValidation;

namespace AutoPartsHub.Application.Auth.Commands;

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

        RuleFor(c => c.Senha)
            .NotEmpty().WithMessage("senha_obrigatoria")
            .MinimumLength(8).WithMessage("senha_minimo_8_caracteres")
            .Matches(@"[a-z]").WithMessage("senha_requer_minuscula")
            .Matches(@"[0-9]").WithMessage("senha_requer_numero");

        RuleFor(c => c.TenantId)
            .NotEmpty().WithMessage("tenant_obrigatorio");

        RuleFor(c => c.Role)
            .NotEmpty().WithMessage("role_obrigatoria")
            .Must(r => new[] { "Admin", "Comprador", "Fornecedor", "Aprovador" }.Contains(r))
            .WithMessage("role_invalida");
    }
}
