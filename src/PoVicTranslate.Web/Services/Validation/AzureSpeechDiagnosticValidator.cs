using Microsoft.Extensions.Options;
using PoVicTranslate.Web.Configuration;
using PoVicTranslate.Web.Models;

namespace PoVicTranslate.Web.Services.Validation;

/// <summary>
/// Validates Azure Speech Service configuration.
/// </summary>
public sealed class AzureSpeechDiagnosticValidator : IDiagnosticValidator
{
    private readonly ApiSettings _settings;

    public string Name => "AzureSpeech";

    public AzureSpeechDiagnosticValidator(IOptions<ApiSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<DiagnosticResult> ValidateAsync()
    {
        var isConfigured = !string.IsNullOrWhiteSpace(_settings.AzureSpeechSubscriptionKey) &&
                           !string.IsNullOrWhiteSpace(_settings.AzureSpeechRegion);

        return Task.FromResult(new DiagnosticResult(
            Name: Name,
            IsConfigured: isConfigured,
            Status: isConfigured ? "Configured" : "Not Configured",
            Message: isConfigured ? null : "Azure Speech settings are missing"));
    }
}
