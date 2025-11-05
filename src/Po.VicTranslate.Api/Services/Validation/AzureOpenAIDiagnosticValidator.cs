using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Po.VicTranslate.Api.Configuration;
using Po.VicTranslate.Api.Models;

namespace Po.VicTranslate.Api.Services.Validation;

/// <summary>
/// Validates Azure OpenAI configuration settings
/// </summary>
public class AzureOpenAIDiagnosticValidator : IDiagnosticValidator
{
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<AzureOpenAIDiagnosticValidator> _logger;

    public string CheckName => "Azure OpenAI Configuration";

    public AzureOpenAIDiagnosticValidator(
        IOptions<ApiSettings> apiSettings,
        ILogger<AzureOpenAIDiagnosticValidator> logger)
    {
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
        _logger = logger;
    }

    public Task<DiagnosticResult> ValidateAsync()
    {
        var result = new DiagnosticResult { CheckName = CheckName };
        _logger.LogInformation("Validating Azure OpenAI configuration...");

        try
        {
            bool keyOk = !string.IsNullOrWhiteSpace(_apiSettings.AzureOpenAIApiKey) &&
                        _apiSettings.AzureOpenAIApiKey != "YOUR_AZURE_OPENAI_API_KEY";
            bool endpointOk = !string.IsNullOrWhiteSpace(_apiSettings.AzureOpenAIEndpoint) &&
                             _apiSettings.AzureOpenAIEndpoint != "YOUR_AZURE_OPENAI_ENDPOINT";
            bool deploymentOk = !string.IsNullOrWhiteSpace(_apiSettings.AzureOpenAIDeploymentName) &&
                               _apiSettings.AzureOpenAIDeploymentName != "YOUR_AZURE_OPENAI_DEPLOYMENT_NAME";

            if (keyOk && endpointOk && deploymentOk)
            {
                result.Success = true;
                result.Message = "Configuration values (Key, Endpoint, Deployment) are present.";
                _logger.LogInformation("Azure OpenAI configuration validation successful.");
            }
            else
            {
                result.Success = false;
                var missing = new List<string>();
                if (!keyOk) missing.Add("ApiKey");
                if (!endpointOk) missing.Add("Endpoint");
                if (!deploymentOk) missing.Add("DeploymentName");

                result.Message = $"Configuration values missing or placeholders: {string.Join(", ", missing)}.";
                _logger.LogWarning("Azure OpenAI configuration validation failed: {Message}", result.Message);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = "An error occurred while validating Azure OpenAI configuration.";
            result.Error = ex;
            _logger.LogError(ex, result.Message);
        }

        return Task.FromResult(result);
    }
}
