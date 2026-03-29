using AutoPartsHub.Domain.Enums;
using FluentResults;

namespace AutoPartsHub.Domain.ValueObjects;

/// <summary>
/// Representa um número de telefone brasileiro (fixo ou celular).
/// Armazena apenas os dígitos, sem formatação.
/// </summary>
public sealed record Telefone
{
    /// <summary>Apenas dígitos do número completo (DDD + número).</summary>
    public string Valor { get; init; }

    /// <summary>Dois primeiros dígitos (DDD).</summary>
    public string DDD => Valor[..2];

    /// <summary>Número sem o DDD.</summary>
    public string Numero => Valor[2..];

    /// <summary>Fixo (10 dígitos) ou Celular (11 dígitos).</summary>
    public TipoTelefone TipoTelefone { get; init; }

    /// <summary>Número formatado: (27) 3333-3333 ou (27) 99999-9999.</summary>
    public string Formatado => TipoTelefone == TipoTelefone.Celular
        ? $"({DDD}) {Numero[..5]}-{Numero[5..]}"
        : $"({DDD}) {Numero[..4]}-{Numero[4..]}";

    private Telefone(string valor, TipoTelefone tipo)
    {
        Valor = valor;
        TipoTelefone = tipo;
    }

    /// <summary>
    /// Cria um Telefone a partir de string com ou sem formatação.
    /// Aceita com ou sem código de país +55.
    /// </summary>
    public static Result<Telefone> Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return Result.Fail<Telefone>("telefone_obrigatorio");

        var digitos = Limpar(valor);

        // Remove código de país +55 (13 dígitos com DDI = 55 + DDD 2 + número 9)
        if (digitos.Length == 12 || digitos.Length == 13)
        {
            if (digitos.StartsWith("55"))
                digitos = digitos[2..];
        }

        if (digitos.Length == 10)
            return Result.Ok(new Telefone(digitos, TipoTelefone.Fixo));

        if (digitos.Length == 11)
            return Result.Ok(new Telefone(digitos, TipoTelefone.Celular));

        return Result.Fail<Telefone>("telefone_deve_ter_10_ou_11_digitos");
    }

    private static string Limpar(string valor)
        => new string(valor.Where(char.IsAsciiDigit).ToArray());
}
