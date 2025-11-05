using Microsoft.ApplicationInsights;
using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.Api.Services.ClientLog;

/// <summary>
/// Factory for selecting the appropriate client log handler based on log level.
/// Implements the Chain of Responsibility pattern by finding the first handler that can process the log entry.
/// </summary>
public class ClientLogHandlerFactory
{
    private readonly IEnumerable<IClientLogHandler> _handlers;

    /// <summary>
    /// Initializes a new instance of the ClientLogHandlerFactory.
    /// </summary>
    /// <param name="handlers">Collection of all registered client log handlers</param>
    /// <exception cref="ArgumentNullException">Thrown when handlers collection is null</exception>
    public ClientLogHandlerFactory(IEnumerable<IClientLogHandler> handlers)
    {
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
    }

    /// <summary>
    /// Gets the appropriate handler for the given log entry.
    /// </summary>
    /// <param name="logEntry">The client log entry to find a handler for</param>
    /// <returns>The first handler that can process the log entry</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler can process the log entry</exception>
    public IClientLogHandler GetHandler(ClientLogEntry logEntry)
    {
        var handler = _handlers.FirstOrDefault(h => h.CanHandle(logEntry));

        if (handler == null)
        {
            throw new InvalidOperationException(
                $"No handler found for log level: {logEntry.Level}");
        }

        return handler;
    }
}
