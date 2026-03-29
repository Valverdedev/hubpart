using FluentResults;

namespace AutoPartsHub.Domain.ValueObjects;

/// <summary>
/// Representa um endereço de e-mail normalizado (minúsculas).
/// Valida formato básico: presença de @ e pelo menos um ponto após o @.
/// </summary>
public sealed record Email
{
    /// <summary>E-mail normalizado em minúsculas.</summary>
    public string Valor { get; init; }

    private Email(string valor)
    {
        Valor = valor;
    }

    /// <summary>Cria um Email a partir de string, normalizando para minúsculas.</summary>
    public static Result<Email> Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return Result.Fail<Email>("email_obrigatorio");

        var normalizado = valor.Trim().ToLowerInvariant();

        if (!EhValido(normalizado))
            return Result.Fail<Email>("email_invalido");

        return Result.Ok(new Email(normalizado));
    }

    /// <summary>Valida formato básico de e-mail: contém @ e pelo menos um ponto após o @.</summary>
    public static bool EhValido(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var indiceArroba = email.IndexOf('@');
        if (indiceArroba <= 0)
            return false;

        var dominio = email[(indiceArroba + 1)..];
        return dominio.Contains('.') && dominio.Length >= 3;
    }
}
