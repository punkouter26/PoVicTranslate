using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.Api.Services.BrowserLog;

/// <summary>
/// Strategy for handling browser instability logs.
/// Processes "instability" log types to track browser-side issues.
/// </summary>
public class InstabilityLogStrategy : IBrowserLogStrategy
{
    /// <inheritdoc />
    public bool CanHandle(string? logType)
    {
        return logType?.ToLower() == "instability";
    }

    /// <inheritdoc />
    public async Task LogAsync(BrowserLogRequest request, IDebugLogService debugService)
    {
        await debugService.LogInstabilityAsync(
            request.Payload?.Component ?? "Browser",
            request.Payload?.Issue ?? "Browser instability",
            request.Payload?.DiagnosticData
        );
    }
}
