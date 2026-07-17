using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Events;

namespace Modules.Common.Application;

/// <summary>
/// Implementation of IEventPublisher that resolves all handlers for an event and executes them in parallel
/// </summary>
public class EventPublisher(IServiceProvider serviceProvider, ILogger<EventPublisher> logger)
    : IEventPublisher
{
    private static readonly Action<ILogger, string, Exception?> PublishingEventMessage =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1, nameof(PublishAsync)),
            "Publishing event {EventType}");

    private static readonly Action<ILogger, string, Exception?> NoHandlersRegisteredMessage =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, nameof(PublishAsync)),
            "No handlers registered for event {EventType}");

    private static readonly Action<ILogger, int, string, Exception?> FoundHandlersMessage =
        LoggerMessage.Define<int, string>(
            LogLevel.Debug,
            new EventId(3, nameof(PublishAsync)),
            "Found {HandlerCount} handlers for event {EventType}");

    private static readonly Action<ILogger, string, Exception?> HandlersThrewExceptionsMessage =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(4, nameof(PublishAsync)),
            "One or more handlers threw exceptions while processing event {EventType}");

    private static readonly Action<ILogger, string, Exception?> SuccessfullyPublishedEventMessage =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(5, nameof(PublishAsync)),
            "Successfully published event {EventType}");

    private static readonly Action<ILogger, string, string, Exception?> ErrorHandlingEventMessage =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(6, nameof(PublishAsync)),
            "Error handling event {EventType} with handler {HandlerType}");

	/// <summary>
	/// Publishes an event of the specified type to all registered event handlers asynchronously.
	/// </summary>
	/// <typeparam name="TEvent">The type of the event to be published, which must implement <see cref="IEvent"/>.</typeparam>
	/// <param name="event">The event instance to be published.</param>
	/// <param name="cancellationToken">A cancellation token to observe during the operation.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <exception cref="AggregateException">
	/// Thrown when one or more handlers throw exceptions during execution. All exceptions
	/// are encapsulated within the <see cref="AggregateException"/>.
	/// </exception>
	/// <exception cref="Exception">Thrown if an error occurs during the publish operation.</exception>
	public async Task PublishAsync<TEvent>(
		TEvent @event,
		CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventType = @event.GetType();
        PublishingEventMessage(logger, eventType.Name, null);

        try
        {
            // Resolve all handlers for this event type
            var handlers = serviceProvider.GetServices<IEventHandler<TEvent>>().ToArray();

            if (handlers.Length == 0)
            {
                NoHandlersRegisteredMessage(logger, eventType.Name, null);
                return;
            }

            FoundHandlersMessage(logger, handlers.Length, eventType.Name, null);

            // Execute all handlers and collect results
            var handlerTasks = handlers
                .Select(handler => ExecuteHandlerAsync(handler, @event, cancellationToken))
                .ToList();

            await Task.WhenAll(handlerTasks);

            // Check for exceptions
            var exceptions = handlerTasks
                .Select(t => t.Exception)
                .Where(ex => ex != null)
                .ToList();

            if (exceptions.Count > 0)
            {
                HandlersThrewExceptionsMessage(logger, eventType.Name, null);
                throw new AggregateException($"One or more handlers threw exceptions while processing event {eventType.Name}", exceptions!);
            }

            SuccessfullyPublishedEventMessage(logger, eventType.Name, null);
        }
        catch (AggregateException)
        {
            // Let the aggregate exception propagate as is
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing event {EventType}", eventType.Name);
            throw;
        }
    }

    private async Task<Exception?> ExecuteHandlerAsync<TEvent>(
        IEventHandler<TEvent> handler,
        TEvent @event,
        CancellationToken cancellationToken) where TEvent : IEvent
    {
        try
        {
            await handler.HandleAsync(@event, cancellationToken);
            return null;
        }
        catch (Exception ex)
        {
            ErrorHandlingEventMessage(logger, @event.GetType().Name, handler.GetType().Name, ex);
            return ex;
        }
    }
}
