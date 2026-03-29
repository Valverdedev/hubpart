using AutoPartsHub.Domain.Enums;
using AutoPartsHub.Domain.ValueObjects;
using FluentValidation;

namespace AutoPartsHub.Application.Tenants.Commands;

public sealed class CadastrarTenantCommandValidator : AbstractValidator<CadastrarTenantCommand>
{
    private static readonly string[] TiposValidos =
        Enum.GetNames<TipoTenant>();

    public CadastrarTenantCommandValidator()
    {
        RuleFor(c => c.RazaoSocial)
            .NotEmpty().WithMessage("razao_social_obrigatoria")
            .MaximumLength(200).WithMessage("razao_social_max_200");

        RuleFor(c => c.NomeFantasia)
            .NotEmpty().WithMessage("nome_fantasia_obrigatorio")
            .MaximumLength(200).WithMessage("nome_fantasia_max_200");

        RuleFor(c => c.Cnpj)
            .NotEmpty().WithMessage("cnpj_obrigatorio")
            .Must(cnpj =>
            {
                var limpo = new string(cnpj.ToUpperInvariant()
                    .Where(c => char.IsAsciiDigit(c) || char.IsAsciiLetterUpper(c))
                    .ToArray());
                return limpo.Length == 14;
            })
            .WithMessage("cnpj_deve_ter_14_caracteres_alfanumericos");

        RuleFor(c => c.Tipo)
            .NotEmpty().WithMessage("tipo_obrigatorio")
            .Must(t => TiposValidos.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("tipo_invalido");

        RuleFor(c => c.EmailContato)
            .NotEmpty().WithMessage("email_obrigatorio")
            .Must(Email.EhValido).WithMessage("email_invalido");

        RuleFor(c => c.Telefones)
            .NotEmpty().WithMessage("telefone_obrigatorio")
            .Must(l => l.Count >= 1 && l.Count <= 5)
            .WithMessage("telefones_min_1_max_5");

        RuleFor(c => c.Endereco.Cep)
            .NotEmpty().WithMessage("cep_obrigatorio")
            .Matches(@"^\d{8}$").WithMessage("cep_deve_ter_8_digitos");

        RuleFor(c => c.Endereco.CodigoIbge)
            .GreaterThan(0).WithMessage("municipio_invalido");

        RuleFor(c => c.Endereco.CodigoUf)
            .GreaterThan(0).WithMessage("estado_invalido");
    }
}
