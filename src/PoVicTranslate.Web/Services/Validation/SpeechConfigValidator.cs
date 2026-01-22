using PoVicTranslate.Web.Configuration;

namespace PoVicTranslate.Web.Services.Validation;

/// <summary>
/// Validates Azure Speech Service configuration.
/// </summary>
public sealed class SpeechConfigValidator : ISpeechConfigValidator
{
    /// <inheritdoc />
    public bool IsValid(ApiSettings settings)
    {
        return !string.IsNullOrWhiteSpace(settings.AzureSpeechSubscriptionKey) &&
               !string.IsNullOrWhiteSpace(settings.AzureSpeechRegion);
    }

    /// <inheritdoc />
    public string GetValidationError(ApiSettings settings)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(settings.AzureSpeechSubscriptionKey))
        {
            errors.Add("AzureSpeechSubscriptionKey is missing");
        }

        if (string.IsNullOrWhiteSpace(settings.AzureSpeechRegion))
        {
            errors.Add("AzureSpeechRegion is missing");
        }

        return string.Join("; ", errors);
    }
}
