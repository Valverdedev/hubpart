using FluentValidation;

namespace AutoPartsHub.Application.Auth.Commands;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(c => c.RefreshToken)
            .NotEmpty().WithMessage("refresh_token_obrigatorio")
            .MaximumLength(512).WithMessage("refresh_token_invalido");
    }
}
