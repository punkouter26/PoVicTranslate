using PoVicTranslate.Web.Services.Validation;

namespace PoVicTranslate.Web.Services;

/// <summary>
/// Validates all service configurations.
/// </summary>
public sealed class ConfigurationValidator : IConfigurationValidator
{
    private readonly IEnumerable<IDiagnosticValidator> _validators;
    private readonly ILogger<ConfigurationValidator> _logger;

    public ConfigurationValidator(
        IEnumerable<IDiagnosticValidator> validators,
        ILogger<ConfigurationValidator> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateAllAsync()
    {
        var allValid = true;

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync();
            if (!result.IsConfigured)
            {
                _logger.LogWarning("Configuration validation failed for {Name}: {Message}",
                    result.Name, result.Message);
                allValid = false;
            }
        }

        return allValid;
    }
}
