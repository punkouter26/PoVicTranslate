using System.Text.RegularExpressions;

namespace PoVicTranslate.Web.Services.Validation;

/// <summary>
/// Validates and sanitizes user input.
/// </summary>
public sealed partial class InputValidator : IInputValidator
{
    /// <inheritdoc />
    public ValidationResult ValidateTextContent(string? text, int maxLength)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
        {
            errors.Add("Text cannot be empty");
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        if (text.Length > maxLength)
        {
            errors.Add($"Text exceeds maximum length of {maxLength} characters");
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        // Sanitize: remove control characters except newlines and tabs
        var sanitized = ControlCharRegex().Replace(text, string.Empty);

        // Check for potentially malicious patterns
        if (ContainsSuspiciousPatterns(sanitized))
        {
            errors.Add("Text contains potentially malicious content");
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        return new ValidationResult
        {
            IsValid = true,
            Errors = errors,
            SanitizedValue = sanitized.Trim()
        };
    }

    private static bool ContainsSuspiciousPatterns(string text)
    {
        // Check for script injection attempts
        var lowerText = text.ToLowerInvariant();
        return lowerText.Contains("<script", StringComparison.Ordinal) ||
               lowerText.Contains("javascript:", StringComparison.Ordinal) ||
               lowerText.Contains("data:", StringComparison.Ordinal);
    }

    [GeneratedRegex(@"[\x00-\x08\x0B\x0C\x0E-\x1F]")]
    private static partial Regex ControlCharRegex();
}
