namespace Po.VicTranslate.Api.Services.Validation;

/// <summary>
/// Service for validating and sanitizing user inputs to prevent security vulnerabilities.
/// </summary>
public interface IInputValidator
{
    /// <summary>
    /// Validates and sanitizes a search query string.
    /// </summary>
    /// <param name="query">The search query to validate.</param>
    /// <returns>A validation result containing the sanitized query and any errors.</returns>
    ValidationResult ValidateSearchQuery(string? query);

    /// <summary>
    /// Validates a resource ID (song ID, artist ID, etc.).
    /// </summary>
    /// <param name="id">The ID to validate.</param>
    /// <returns>A validation result containing the sanitized ID and any errors.</returns>
    ValidationResult ValidateResourceId(string? id);

    /// <summary>
    /// Validates text content for translation or speech synthesis.
    /// </summary>
    /// <param name="text">The text to validate.</param>
    /// <param name="maxLength">Maximum allowed length in characters.</param>
    /// <returns>A validation result containing the sanitized text and any errors.</returns>
    ValidationResult ValidateTextContent(string? text, int maxLength = 5000);

    /// <summary>
    /// Validates a file path to prevent directory traversal attacks.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <param name="allowedExtensions">List of allowed file extensions (e.g., [".txt", ".json"]).</param>
    /// <returns>A validation result containing the sanitized path and any errors.</returns>
    ValidationResult ValidateFilePath(string? filePath, string[]? allowedExtensions = null);

    /// <summary>
    /// Validates a pagination parameter (page number, max results, etc.).
    /// </summary>
    /// <param name="value">The numeric value to validate.</param>
    /// <param name="min">Minimum allowed value.</param>
    /// <param name="max">Maximum allowed value.</param>
    /// <param name="parameterName">Name of the parameter for error messages.</param>
    /// <returns>A validation result containing the validated value and any errors.</returns>
    ValidationResult<int> ValidateNumericParameter(int value, int min, int max, string parameterName);
}

/// <summary>
/// Result of input validation.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? SanitizedValue { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ValidationResult Success(string sanitizedValue) =>
        new() { IsValid = true, SanitizedValue = sanitizedValue };

    public static ValidationResult Failure(params string[] errors) =>
        new() { IsValid = false, Errors = errors.ToList() };
}

/// <summary>
/// Generic validation result for typed values.
/// </summary>
public class ValidationResult<T>
{
    public bool IsValid { get; set; }
    public T? Value { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ValidationResult<T> Success(T value) =>
        new() { IsValid = true, Value = value };

    public static ValidationResult<T> Failure(params string[] errors) =>
        new() { IsValid = false, Errors = errors.ToList() };
}
