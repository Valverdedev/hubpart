using FluentResults;

namespace AutoPartsHub.API.Extensions;

public static class ResultExtensions
{
    /// <summary>Converte Result&lt;T&gt; em 200 Ok ou 422 Unprocessable.</summary>
    public static IResult ParaIResult<T>(this Result<T> resultado)
    {
        if (resultado.IsSuccess)
            return Results.Ok(resultado.Value);

        return Results.UnprocessableEntity(new { erros = resultado.Errors.Select(e => e.Message) });
    }

    /// <summary>
    /// Converte Result&lt;T&gt; em 201 Created (com Location) ou 422 Unprocessable.
    /// A URI é calculada via lambda apenas quando IsSuccess — evita acessar Value em resultado falho.
    /// </summary>
    public static IResult ParaIResultCriado<T>(this Result<T> resultado, Func<T, string> obterUri)
    {
        if (resultado.IsSuccess)
            return Results.Created(obterUri(resultado.Value), new { id = resultado.Value });

        return Results.UnprocessableEntity(new { erros = resultado.Errors.Select(e => e.Message) });
    }

    /// <summary>Converte Result (não genérico) em 204 No Content ou 422 Unprocessable.</summary>
    public static IResult ParaIResult(this Result resultado)
    {
        if (resultado.IsSuccess)
            return Results.NoContent();

        return Results.UnprocessableEntity(new { erros = resultado.Errors.Select(e => e.Message) });
    }
}
