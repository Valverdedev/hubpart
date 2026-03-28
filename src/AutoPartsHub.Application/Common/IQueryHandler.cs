using FluentResults;
using MediatR;

namespace AutoPartsHub.Application.Common;

/// <summary>Handler para query — apenas lê dados, nunca altera estado.</summary>
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
