using PoVicTranslate.Web.Models;
using PoVicTranslate.Web.Services.Validation;

namespace PoVicTranslate.Web.Services;

/// <summary>
/// Runs diagnostic checks on all services.
/// </summary>
public sealed class DiagnosticService : IDiagnosticService
{
    private readonly IEnumerable<IDiagnosticValidator> _validators;

    public DiagnosticService(IEnumerable<IDiagnosticValidator> validators)
    {
        _validators = validators;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DiagnosticResult>> RunDiagnosticsAsync()
    {
        var results = new List<DiagnosticResult>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync();
            results.Add(result);
        }

        return results;
    }
}
