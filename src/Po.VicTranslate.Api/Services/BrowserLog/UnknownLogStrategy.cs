using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.Api.Services.BrowserLog;

/// <summary>
/// Fallback strategy for handling unknown browser log types.
/// Logs unknown events for diagnostic purposes.
/// </summary>
public class UnknownLogStrategy : IBrowserLogStrategy
{
    /// <inheritdoc />
    public bool CanHandle(string? logType)
    {
        // This is the fallback strategy - always returns true
        return true;
    }

    /// <inheritdoc />
    public async Task LogAsync(BrowserLogRequest request, IDebugLogService debugService)
    {
        await debugService.LogEventAsync(
            "UnknownBrowserEvent",
            $"Unknown browser log type: {request.Type}",
            request.Payload
        );
    }
}
