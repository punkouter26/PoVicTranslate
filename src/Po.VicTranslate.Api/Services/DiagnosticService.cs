using Microsoft.Extensions.Logging;
using Po.VicTranslate.Api.Models;

namespace Po.VicTranslate.Api.Services;

/// <summary>
/// Service to orchestrate diagnostic checks on application dependencies.
/// </summary>
public class DiagnosticService : IDiagnosticService
{
    private readonly IConfigurationValidator _configurationValidator;
    private readonly ILogger<DiagnosticService> _logger;

    public DiagnosticService(IConfigurationValidator configurationValidator, ILogger<DiagnosticService> logger)
    {
        _configurationValidator = configurationValidator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<DiagnosticResult>> RunChecksAsync()
    {
        _logger.LogInformation("Running diagnostic checks...");
        var results = new List<DiagnosticResult>();

        // Run all validation checks
        results.Add(_configurationValidator.ValidateAzureOpenAI());
        results.Add(await _configurationValidator.ValidateAzureSpeechAsync());
        results.Add(_configurationValidator.ValidateInternetConnectivity());

        _logger.LogInformation("Diagnostic checks completed.");
        return results;
    }
}
