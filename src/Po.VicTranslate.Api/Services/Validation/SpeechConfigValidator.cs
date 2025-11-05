using Po.VicTranslate.Api.Configuration;

namespace Po.VicTranslate.Api.Services.Validation;

/// <summary>
/// Validates Azure Speech Service configuration settings
/// </summary>
public class SpeechConfigValidator : ISpeechConfigValidator
{
    public bool IsValid(ApiSettings settings)
    {
        if (settings == null)
            return false;

        return !string.IsNullOrWhiteSpace(settings.AzureSpeechSubscriptionKey) &&
               !string.IsNullOrWhiteSpace(settings.AzureSpeechRegion);
    }

    public string GetValidationError(ApiSettings settings)
    {
        if (settings == null)
            return "Settings cannot be null";

        if (string.IsNullOrWhiteSpace(settings.AzureSpeechSubscriptionKey))
            return "Azure Speech SubscriptionKey is missing or empty";

        if (string.IsNullOrWhiteSpace(settings.AzureSpeechRegion))
            return "Azure Speech Region is missing or empty";

        return string.Empty;
    }
}
