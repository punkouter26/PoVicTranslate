namespace PoVicTranslate.Web.Services;

/// <summary>
/// Interface for diagnostic service.
/// </summary>
public interface IDiagnosticService
{
    /// <summary>
    /// Runs all diagnostic checks.
    /// </summary>
    Task<IEnumerable<Models.DiagnosticResult>> RunDiagnosticsAsync();
}
