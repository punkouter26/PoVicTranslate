using Po.VicTranslate.Api.Models;

namespace Po.VicTranslate.Api.Services.Validation;

/// <summary>
/// Interface for validating a specific Azure service configuration and connectivity
/// </summary>
public interface IDiagnosticValidator
{
    /// <summary>
    /// Gets the name of the check being performed
    /// </summary>
    string CheckName { get; }

    /// <summary>
    /// Validates the service configuration and connectivity
    /// </summary>
    /// <returns>Diagnostic result with success status and details</returns>
    Task<DiagnosticResult> ValidateAsync();
}
