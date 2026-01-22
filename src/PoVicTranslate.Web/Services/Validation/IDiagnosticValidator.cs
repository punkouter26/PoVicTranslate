using PoVicTranslate.Web.Models;

namespace PoVicTranslate.Web.Services.Validation;

/// <summary>
/// Interface for diagnostic validators.
/// </summary>
public interface IDiagnosticValidator
{
    /// <summary>
    /// Gets the name of this diagnostic check.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Validates the service configuration.
    /// </summary>
    Task<DiagnosticResult> ValidateAsync();
}
