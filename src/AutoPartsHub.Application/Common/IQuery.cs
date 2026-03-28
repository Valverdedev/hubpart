using FluentResults;
using MediatR;

namespace AutoPartsHub.Application.Common;

/// <summary>
/// Marca uma query que retorna dados sem produzir efeitos colaterais.
/// Queries nunca alteram estado — apenas leem.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
