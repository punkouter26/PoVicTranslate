namespace PoVicTranslate.Web.Services;

/// <summary>
/// Interface for configuration validator.
/// </summary>
public interface IConfigurationValidator
{
    /// <summary>
    /// Validates all service configurations.
    /// </summary>
    Task<bool> ValidateAllAsync();
}
