using PoVicTranslate.Web.Models;

namespace PoVicTranslate.Web.Services.Validation;

/// <summary>
/// Validates internet connectivity.
/// </summary>
public sealed class InternetConnectivityDiagnosticValidator : IDiagnosticValidator
{
    private readonly IHttpClientFactory _httpClientFactory;

    public string Name => "InternetConnectivity";

    public InternetConnectivityDiagnosticValidator(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<DiagnosticResult> ValidateAsync()
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            var response = await client.GetAsync("https://www.microsoft.com");
            var isConnected = response.IsSuccessStatusCode;

            return new DiagnosticResult(
                Name: Name,
                IsConfigured: true,
                Status: isConnected ? "Connected" : "Disconnected",
                Message: isConnected ? null : "Unable to reach external services");
        }
        catch (Exception ex)
        {
            return new DiagnosticResult(
                Name: Name,
                IsConfigured: true,
                Status: "Error",
                Message: $"Connectivity check failed: {ex.Message}");
        }
    }
}
