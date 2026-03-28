using AutoPartsHub.Application.Common;
using AutoPartsHub.Domain.Entidades;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AutoPartsHub.Application.Auth.Commands;

// ---------------------------------------------------------------------------
// Command
// ---------------------------------------------------------------------------

/// <summary>
/// Registra um novo usuário vinculado a um tenant.
/// Usado para criação de conta via API — não expõe senha em texto plano na resposta.
/// </summary>
public record RegistroCommand(
    string NomeCompleto,
    string Email,
    string Senha,
    Guid TenantId,
    string Role
) : ICommand<Guid>;

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
    UserManager<UsuarioApp> userManager
) : ICommandHandler<RegistroCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegistroCommand command, CancellationToken ct)
    {
        // 1. Verifica se e-mail já está em uso (sem filtro de tenant — email é global)
        var existente = await userManager.FindByEmailAsync(command.Email);
        if (existente is not null)
            return Result.Fail<Guid>("email_ja_cadastrado");

        // 2. Cria a entidade
        var usuario = new UsuarioApp
        {
            Id = Guid.NewGuid(),
            TenantId = command.TenantId,
            NomeCompleto = command.NomeCompleto,
            Email = command.Email,
            UserName = command.Email,
        };

        // 3. Persiste com hash de senha gerado pelo Identity
        var resultado = await userManager.CreateAsync(usuario, command.Senha);
        if (!resultado.Succeeded)
        {
            var erros = resultado.Errors.Select(e => e.Description);
            return Result.Fail<Guid>(string.Join(" | ", erros));
        }

        // 4. Atribui a role
        var resultadoRole = await userManager.AddToRoleAsync(usuario, command.Role);
        if (!resultadoRole.Succeeded)
        {
            var erros = resultadoRole.Errors.Select(e => e.Description);
            return Result.Fail<Guid>(string.Join(" | ", erros));
        }

        return Result.Ok(usuario.Id);
    }
}
