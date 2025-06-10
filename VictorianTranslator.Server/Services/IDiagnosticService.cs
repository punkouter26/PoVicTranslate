using VictorianTranslator.Models; // Assuming DiagnosticResult will be in a Models folder

namespace VictorianTranslator.Services;

/// <summary>
/// Defines the contract for a service that performs diagnostic checks
/// on external dependencies and system components.
/// </summary>
public interface IDiagnosticService
{
    /// <summary>
    /// Runs all configured diagnostic checks asynchronously.
    /// </summary>
    /// <returns>A list of DiagnosticResult objects detailing the outcome of each check.</returns>
    Task<List<DiagnosticResult>> RunChecksAsync();
}
