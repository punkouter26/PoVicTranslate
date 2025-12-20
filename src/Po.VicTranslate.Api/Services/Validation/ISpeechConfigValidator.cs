using Po.VicTranslate.Api.Configuration;

namespace Po.VicTranslate.Api.Services.Validation;

/// <summary>
/// Interface for validating Azure Speech Service configuration
/// </summary>
public interface ISpeechConfigValidator
{
    /// <summary>
    /// Validates that required Speech Service settings are present and not placeholders
    /// </summary>
    /// <param name="settings">The API settings to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool IsValid(ApiSettings settings);

    /// <summary>
    /// Gets a descriptive error message for invalid settings
    /// </summary>
    string GetValidationError(ApiSettings settings);
}
