using FluentResults;
using MediatR;

namespace AutoPartsHub.Application.Common;

/// <summary>Handler para command sem valor de retorno.</summary>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

/// <summary>Handler para command que retorna um valor em caso de sucesso.</summary>
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
