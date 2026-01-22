using Microsoft.Extensions.Options;
using PoVicTranslate.Web.Configuration;
using PoVicTranslate.Web.Models;

namespace PoVicTranslate.Web.Services.Validation;

/// <summary>
/// Validates Azure OpenAI configuration.
/// </summary>
public sealed class AzureOpenAIDiagnosticValidator : IDiagnosticValidator
{
    private readonly ApiSettings _settings;

    public string Name => "AzureOpenAI";

    public AzureOpenAIDiagnosticValidator(IOptions<ApiSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<DiagnosticResult> ValidateAsync()
    {
        var isConfigured = !string.IsNullOrWhiteSpace(_settings.AzureOpenAIApiKey) &&
                           !string.IsNullOrWhiteSpace(_settings.AzureOpenAIEndpoint) &&
                           !string.IsNullOrWhiteSpace(_settings.AzureOpenAIDeploymentName);

        return Task.FromResult(new DiagnosticResult(
            Name: Name,
            IsConfigured: isConfigured,
            Status: isConfigured ? "Configured" : "Not Configured",
            Message: isConfigured ? null : "Azure OpenAI settings are missing"));
    }
}
