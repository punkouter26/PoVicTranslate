using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.Api.Services.BrowserLog;

/// <summary>
/// Strategy for handling browser event logs.
/// Processes "event" and "browser-event" log types.
/// </summary>
public class EventLogStrategy : IBrowserLogStrategy
{
    /// <inheritdoc />
    public bool CanHandle(string? logType)
    {
        var normalized = logType?.ToLower();
        return normalized is "event" or "browser-event";
    }

    /// <inheritdoc />
    public async Task LogAsync(BrowserLogRequest request, IDebugLogService debugService)
    {
        await debugService.LogEventAsync(
            request.Payload?.EventType ?? "BrowserEvent",
            request.Payload?.Message ?? "Browser event",
            request.Payload?.Data
        );
    }
}
