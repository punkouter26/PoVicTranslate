namespace PoVicTranslate.Web.Services;

/// <summary>
/// Interface for custom telemetry tracking.
/// </summary>
public interface ICustomTelemetryService
{
    /// <summary>
    /// Tracks a translation request.
    /// </summary>
    void TrackTranslationRequest(string inputLanguage, int textLength, bool success, long durationMs);

    /// <summary>
    /// Tracks user activity.
    /// </summary>
    void TrackUserActivity(string activity, string? userId);
}
