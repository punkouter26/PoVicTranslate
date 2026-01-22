using Microsoft.ApplicationInsights;

namespace PoVicTranslate.Web.Services;

/// <summary>
/// Custom telemetry service for tracking application metrics.
/// </summary>
public sealed class CustomTelemetryService : ICustomTelemetryService
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<CustomTelemetryService> _logger;

    public CustomTelemetryService(TelemetryClient telemetryClient, ILogger<CustomTelemetryService> logger)
    {
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public void TrackTranslationRequest(string inputLanguage, int textLength, bool success, long durationMs)
    {
        _telemetryClient.TrackEvent("Translation_Request", new Dictionary<string, string>
        {
            ["InputLanguage"] = inputLanguage,
            ["TextLength"] = textLength.ToString(),
            ["Success"] = success.ToString(),
            ["DurationMs"] = durationMs.ToString()
        });

        _telemetryClient.TrackMetric("Translation_Duration_Ms", durationMs);
        _telemetryClient.TrackMetric("Translation_TextLength", textLength);

        _logger.LogDebug("Tracked translation request: {Language}, {Length} chars, {Success}, {Duration}ms",
            inputLanguage, textLength, success, durationMs);
    }

    /// <inheritdoc />
    public void TrackUserActivity(string activity, string? userId)
    {
        _telemetryClient.TrackEvent("User_Activity", new Dictionary<string, string>
        {
            ["Activity"] = activity,
            ["UserId"] = userId ?? "anonymous"
        });

        _logger.LogDebug("Tracked user activity: {Activity} for {UserId}", activity, userId ?? "anonymous");
    }
}
