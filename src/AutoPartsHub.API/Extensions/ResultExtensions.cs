using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace AutoPartsHub.API.Extensions;

public static class ResultExtensions
{
    /// <summary>Converte Result&lt;T&gt; em 200 Ok ou 422 Unprocessable.</summary>
    public static IActionResult ParaActionResult<T>(this Result<T> resultado, ControllerBase controller)
    {
        if (resultado.IsSuccess)
            return controller.Ok(resultado.Value);

        return controller.UnprocessableEntity(new { erros = resultado.Errors.Select(e => e.Message) });
    }

    /// <summary>
    /// Converte Result&lt;T&gt; em 201 Created (com Location) ou 422 Unprocessable.
    /// </summary>
    public static IActionResult ParaActionResultCriado<T>(
        this Result<T> resultado,
        ControllerBase controller,
        string nomeRota,
        Func<T, object> obterRotaValores)
    {
        if (resultado.IsSuccess)
            return controller.CreatedAtRoute(nomeRota, obterRotaValores(resultado.Value), new { id = resultado.Value });

        return controller.UnprocessableEntity(new { erros = resultado.Errors.Select(e => e.Message) });
    }

    /// <summary>Converte Result (não genérico) em 204 No Content ou 422 Unprocessable.</summary>
    public static IActionResult ParaActionResult(this Result resultado, ControllerBase controller)
    {
        if (resultado.IsSuccess)
            return controller.NoContent();

        return controller.UnprocessableEntity(new { erros = resultado.Errors.Select(e => e.Message) });
    }
}
