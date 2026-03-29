using FluentResults;

namespace AutoPartsHub.Domain.ValueObjects;

/// <summary>
/// Representa um CNPJ alfanumérico (nova regra Receita Federal 2026).
/// Armazena sempre 14 caracteres limpos (sem formatação).
/// A validação de dígito verificador é feita via ICnpjService — este VO valida apenas estrutura.
/// </summary>
public sealed record Cnpj
{
    /// <summary>CNPJ limpo, 14 caracteres alfanuméricos em maiúsculas.</summary>
    public string Valor { get; init; }

    /// <summary>
    /// CNPJ formatado com máscara XX.XXX.XXX/XXXX-XX apenas quando numérico puro.
    /// Para CNPJs alfanuméricos, retorna o valor limpo sem máscara.
    /// </summary>
    public string Formatado => EhAlfanumerico()
        ? Valor
        : $"{Valor[..2]}.{Valor[2..5]}.{Valor[5..8]}/{Valor[8..12]}-{Valor[12..14]}";

    private Cnpj(string valor)
    {
        Valor = valor;
    }

    /// <summary>Cria um Cnpj a partir de string com ou sem formatação.</summary>
    public static Result<Cnpj> Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return Result.Fail<Cnpj>("cnpj_obrigatorio");

        var limpo = Limpar(valor);

        if (limpo.Length != 14)
            return Result.Fail<Cnpj>("cnpj_deve_ter_14_caracteres");

        if (!limpo.All(c => char.IsAsciiDigit(c) || char.IsAsciiLetterUpper(c)))
            return Result.Fail<Cnpj>("cnpj_caracteres_invalidos");

        return Result.Ok(new Cnpj(limpo));
    }

    /// <summary>Indica se o CNPJ contém letras (formato alfanumérico novo).</summary>
    public bool EhAlfanumerico() => Valor.Any(char.IsLetter);

    private static string Limpar(string valor)
        => new string(valor.ToUpperInvariant()
            .Where(c => char.IsAsciiDigit(c) || char.IsAsciiLetterUpper(c))
            .ToArray());
}
