using PoVicTranslate.Web.Configuration;

namespace PoVicTranslate.Web.Services.Validation;

/// <summary>
/// Interface for validating speech configuration.
/// </summary>
public interface ISpeechConfigValidator
{
    /// <summary>
    /// Checks if the configuration is valid.
    /// </summary>
    bool IsValid(ApiSettings settings);

    /// <summary>
    /// Gets the validation error message.
    /// </summary>
    string GetValidationError(ApiSettings settings);
}
