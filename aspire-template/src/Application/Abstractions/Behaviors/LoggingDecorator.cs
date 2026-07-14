using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal static partial class LoggingDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;

            LogProcessingCommand(logger, commandName);

            Result<TResponse> result = await innerHandler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                LogCompletedCommand(logger, commandName);
            }
            else
            {
                LogCompletedCommandWithError(logger, commandName, result.Error);
            }

            return result;
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandBaseHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;

            LogProcessingCommand(logger, commandName);

            Result result = await innerHandler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                LogCompletedCommand(logger, commandName);
            }
            else
            {
                LogCompletedCommandWithError(logger, commandName, result.Error);
            }

            return result;
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            string queryName = typeof(TQuery).Name;

            LogProcessingQuery(logger, queryName);

            Result<TResponse> result = await innerHandler.Handle(query, cancellationToken);

            if (result.IsSuccess)
            {
                LogCompletedQuery(logger, queryName);
            }
            else
            {
                LogCompletedQueryWithError(logger, queryName, result.Error);
            }

            return result;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing command {CommandName}")]
    private static partial void LogProcessingCommand(ILogger logger, string commandName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Completed command {CommandName}")]
    private static partial void LogCompletedCommand(ILogger logger, string commandName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Completed command {CommandName} with error: {Error}")]
    private static partial void LogCompletedCommandWithError(ILogger logger, string commandName, Error error);

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing query {QueryName}")]
    private static partial void LogProcessingQuery(ILogger logger, string queryName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Completed query {QueryName}")]
    private static partial void LogCompletedQuery(ILogger logger, string queryName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Completed query {QueryName} with error: {Error}")]
    private static partial void LogCompletedQueryWithError(ILogger logger, string queryName, Error error);
}
