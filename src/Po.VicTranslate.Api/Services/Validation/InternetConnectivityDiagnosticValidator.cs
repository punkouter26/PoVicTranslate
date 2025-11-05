using Microsoft.Extensions.Logging;
using Po.VicTranslate.Api.Models;

namespace Po.VicTranslate.Api.Services.Validation;

/// <summary>
/// Validates internet connectivity
/// </summary>
public class InternetConnectivityDiagnosticValidator : IDiagnosticValidator
{
    private readonly ILogger<InternetConnectivityDiagnosticValidator> _logger;

    public string CheckName => "Internet Connectivity";

    public InternetConnectivityDiagnosticValidator(ILogger<InternetConnectivityDiagnosticValidator> logger)
    {
        _logger = logger;
    }

    public Task<DiagnosticResult> ValidateAsync()
    {
        var result = new DiagnosticResult { CheckName = CheckName };
        _logger.LogInformation("Validating internet connectivity...");

        try
        {
            using var ping = new System.Net.NetworkInformation.Ping();
            var reply = ping.Send("8.8.8.8", 2000);

            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                result.Success = true;
                result.Message = "Internet connection appears to be available.";
                _logger.LogInformation(result.Message);
            }
            else
            {
                result.Success = false;
                result.Message = $"Ping failed. Status: {reply.Status}.";
                _logger.LogWarning("Internet connectivity validation failed: {Message}", result.Message);
            }
        }
        catch (System.Net.NetworkInformation.PingException pex)
        {
            result.Success = false;
            result.Message = $"Ping failed with exception: {pex.Message}";
            result.Error = pex;
            _logger.LogError(pex, "Internet connectivity validation failed with PingException.");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"An unexpected error occurred during internet check: {ex.Message}";
            result.Error = ex;
            _logger.LogError(ex, "Internet connectivity validation failed with unexpected exception.");
        }

        return Task.FromResult(result);
    }
}
