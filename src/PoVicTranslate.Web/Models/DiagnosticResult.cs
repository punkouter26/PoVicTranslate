namespace PoVicTranslate.Web.Models;

/// <summary>
/// Response model for diagnostic results.
/// </summary>
public sealed record DiagnosticResult(
    string Name,
    bool IsConfigured,
    string Status,
    string? Message);
