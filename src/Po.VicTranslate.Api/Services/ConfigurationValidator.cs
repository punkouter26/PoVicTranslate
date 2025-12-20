using Microsoft.Extensions.Logging;
using Po.VicTranslate.Api.Models;
using Po.VicTranslate.Api.Services.Validation;

namespace Po.VicTranslate.Api.Services;

/// <summary>
/// Service for validating application configuration settings
/// Delegates to specific validators following Single Responsibility Principle
/// </summary>
public class ConfigurationValidator : IConfigurationValidator
{
    private readonly IEnumerable<IDiagnosticValidator> _validators;
    private readonly ILogger<ConfigurationValidator> _logger;

    public ConfigurationValidator(
        IEnumerable<IDiagnosticValidator> validators,
        ILogger<ConfigurationValidator> logger)
    {
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        _logger = logger;
    }

    public DiagnosticResult ValidateAzureOpenAI()
    {
        _logger.LogInformation("Delegating Azure OpenAI validation...");
        var validator = _validators.FirstOrDefault(v => v.CheckName == "Azure OpenAI Configuration");

        if (validator == null)
        {
            _logger.LogWarning("Azure OpenAI validator not found in registered validators");
            return new DiagnosticResult
            {
                CheckName = "Azure OpenAI Configuration",
                Success = false,
                Message = "Validator not registered"
            };
        }

        return validator.ValidateAsync().GetAwaiter().GetResult();
    }

    public async Task<DiagnosticResult> ValidateAzureSpeechAsync()
    {
        _logger.LogInformation("Delegating Azure Speech validation...");
        var validator = _validators.FirstOrDefault(v => v.CheckName == "Azure Speech Service Connection");

        if (validator == null)
        {
            _logger.LogWarning("Azure Speech validator not found in registered validators");
            return new DiagnosticResult
            {
                CheckName = "Azure Speech Service Connection",
                Success = false,
                Message = "Validator not registered"
            };
        }

        return await validator.ValidateAsync();
    }

    public DiagnosticResult ValidateInternetConnectivity()
    {
        _logger.LogInformation("Delegating Internet connectivity validation...");
        var validator = _validators.FirstOrDefault(v => v.CheckName == "Internet Connectivity");

        if (validator == null)
        {
            _logger.LogWarning("Internet connectivity validator not found in registered validators");
            return new DiagnosticResult
            {
                CheckName = "Internet Connectivity",
                Success = false,
                Message = "Validator not registered"
            };
        }

        return validator.ValidateAsync().GetAwaiter().GetResult();
    }
}
