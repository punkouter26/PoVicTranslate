namespace Po.VicTranslate.Api.Models;

/// <summary>
/// Represents the result of a single diagnostic check.
/// </summary>
public class DiagnosticResult
{
    /// <summary>
    /// Gets or sets the name of the check performed (e.g., "Azure OpenAI Config", "Azure Speech Connection").
    /// </summary>
    public string CheckName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the check was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a message providing details about the check's outcome (e.g., "Configuration OK", "Error: API key missing", "Connection successful").
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional exception if the check failed due to an error.
    /// </summary>
    public Exception? Error { get; set; }
}
