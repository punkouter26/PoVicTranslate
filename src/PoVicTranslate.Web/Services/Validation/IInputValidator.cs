namespace PoVicTranslate.Web.Services.Validation;

/// <summary>
/// Interface for input validation.
/// </summary>
public interface IInputValidator
{
    /// <summary>
    /// Validates text content.
    /// </summary>
    ValidationResult ValidateTextContent(string? text, int maxLength);
}

/// <summary>
/// Result of input validation.
/// </summary>
public sealed class ValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = [];
    public string? SanitizedValue { get; init; }
}
