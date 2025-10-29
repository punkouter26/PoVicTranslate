using Po.VicTranslate.Api.Models;

namespace Po.VicTranslate.Api.Services;

/// <summary>
/// Defines the contract for validating application configuration settings
/// </summary>
public interface IConfigurationValidator
{
    /// <summary>
    /// Validates Azure OpenAI configuration settings
    /// </summary>
    DiagnosticResult ValidateAzureOpenAI();

    /// <summary>
    /// Validates Azure Speech Service configuration and connectivity
    /// </summary>
    Task<DiagnosticResult> ValidateAzureSpeechAsync();

    /// <summary>
    /// Validates basic internet connectivity
    /// </summary>
    DiagnosticResult ValidateInternetConnectivity();
}
