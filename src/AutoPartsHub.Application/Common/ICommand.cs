using FluentResults;
using MediatR;

namespace AutoPartsHub.Application.Common;

/// <summary>Marca um command que não retorna valor além do sucesso/falha.</summary>
public interface ICommand : IRequest<Result>;

/// <summary>Marca um command que retorna um valor em caso de sucesso.</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
