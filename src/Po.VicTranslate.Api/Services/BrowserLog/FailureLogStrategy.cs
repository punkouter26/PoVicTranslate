using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.Api.Services.BrowserLog;

/// <summary>
/// Strategy for handling browser failure logs.
/// Processes "failure" log types to track critical browser-side failures.
/// </summary>
public class FailureLogStrategy : IBrowserLogStrategy
{
    /// <inheritdoc />
    public bool CanHandle(string? logType)
    {
        return logType?.ToLower() == "failure";
    }

    /// <inheritdoc />
    public async Task LogAsync(BrowserLogRequest request, IDebugLogService debugService)
    {
        await debugService.LogStructuralFailureAsync(
            request.Payload?.Component ?? "Browser",
            request.Payload?.Failure ?? "Browser failure",
            null,
            request.Payload?.Context
        );
    }
}
