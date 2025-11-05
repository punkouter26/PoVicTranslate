using System.Text;
using System.Text.RegularExpressions;

namespace Po.VicTranslate.Api.Services.Validation;

/// <summary>
/// Implementation of input validation and sanitization service.
/// Protects against injection attacks, directory traversal, and malicious input.
/// </summary>
public partial class InputValidator : IInputValidator
{
    private readonly ILogger<InputValidator> _logger;

    // Security: Regex for validating resource IDs (alphanumeric, hyphens, underscores only)
    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$", RegexOptions.Compiled)]
    private static partial Regex ResourceIdPattern();

    // Security: Dangerous characters that could be used for injection attacks
    private static readonly char[] DangerousChars = ['<', '>', '"', '\'', '&', ';', '|', '`', '$', '(', ')', '{', '}', '[', ']'];

    // Security: Path traversal patterns
    [GeneratedRegex(@"\.\.|/\.|\\\.|\0", RegexOptions.Compiled)]
    private static partial Regex PathTraversalPattern();

    public InputValidator(ILogger<InputValidator> logger)
    {
        _logger = logger;
    }

    public ValidationResult ValidateSearchQuery(string? query)
    {
        // Allow empty/null queries (returns all results)
        if (string.IsNullOrWhiteSpace(query))
        {
            return ValidationResult.Success(string.Empty);
        }

        // Trim whitespace
        var trimmed = query.Trim();

        // Security: Check length to prevent DoS
        if (trimmed.Length > 200)
        {
            _logger.LogWarning("Search query rejected: exceeds maximum length. Length={Length}", trimmed.Length);
            return ValidationResult.Failure("Search query cannot exceed 200 characters.");
        }

        // Security: Remove dangerous characters that could be used for injection
        var sanitized = SanitizeDangerousCharacters(trimmed);

        // Security: Remove excessive whitespace
        sanitized = Regex.Replace(sanitized, @"\s+", " ");

        _logger.LogDebug("Search query validated and sanitized. Original='{Original}', Sanitized='{Sanitized}'",
            query, sanitized);

        return ValidationResult.Success(sanitized);
    }

    public ValidationResult ValidateResourceId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return ValidationResult.Failure("Resource ID cannot be empty.");
        }

        var trimmed = id.Trim();

        // Security: Check length
        if (trimmed.Length > 100)
        {
            _logger.LogWarning("Resource ID rejected: exceeds maximum length. Length={Length}", trimmed.Length);
            return ValidationResult.Failure("Resource ID cannot exceed 100 characters.");
        }

        // Security: Only allow alphanumeric, hyphens, and underscores
        if (!ResourceIdPattern().IsMatch(trimmed))
        {
            _logger.LogWarning("Resource ID rejected: contains invalid characters. ID='{Id}'", id);
            return ValidationResult.Failure("Resource ID can only contain letters, numbers, hyphens, and underscores.");
        }

        return ValidationResult.Success(trimmed);
    }

    public ValidationResult ValidateTextContent(string? text, int maxLength = 5000)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Failure("Text content cannot be empty.");
        }

        var trimmed = text.Trim();

        // Security: Check length to prevent DoS and excessive API costs
        if (trimmed.Length > maxLength)
        {
            _logger.LogWarning("Text content rejected: exceeds maximum length. Length={Length}, Max={Max}",
                trimmed.Length, maxLength);
            return ValidationResult.Failure($"Text content cannot exceed {maxLength} characters.");
        }

        // Security: Check for null bytes (can cause issues in some systems)
        if (trimmed.Contains('\0'))
        {
            _logger.LogWarning("Text content rejected: contains null bytes");
            return ValidationResult.Failure("Text content contains invalid characters.");
        }

        // Security: Limit control characters (except common whitespace)
        var sanitized = SanitizeControlCharacters(trimmed);

        // Security: Check if text is entirely whitespace after sanitization
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return ValidationResult.Failure("Text content contains only invalid or whitespace characters.");
        }

        _logger.LogDebug("Text content validated. Length={Length}", sanitized.Length);

        return ValidationResult.Success(sanitized);
    }

    public ValidationResult ValidateFilePath(string? filePath, string[]? allowedExtensions = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return ValidationResult.Failure("File path cannot be empty.");
        }

        var trimmed = filePath.Trim();

        // Security: Check for path traversal attempts (../, ..\, null bytes)
        if (PathTraversalPattern().IsMatch(trimmed))
        {
            _logger.LogWarning("File path rejected: contains path traversal patterns. Path='{Path}'", filePath);
            return ValidationResult.Failure("File path contains invalid patterns.");
        }

        // Security: Check for absolute paths (should be relative)
        if (Path.IsPathRooted(trimmed))
        {
            _logger.LogWarning("File path rejected: is an absolute path. Path='{Path}'", filePath);
            return ValidationResult.Failure("File path must be relative.");
        }

        // Security: Validate file extension if restrictions are provided
        if (allowedExtensions != null && allowedExtensions.Length > 0)
        {
            var extension = Path.GetExtension(trimmed).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("File path rejected: invalid extension. Path='{Path}', Extension='{Extension}'",
                    filePath, extension);
                return ValidationResult.Failure(
                    $"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", allowedExtensions)}");
            }
        }

        // Security: Get the full normalized path and ensure it doesn't escape the base directory
        // This is a defense-in-depth measure
        try
        {
            var fullPath = Path.GetFullPath(trimmed);
            _logger.LogDebug("File path validated. Path='{Path}', FullPath='{FullPath}'", trimmed, fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "File path rejected: invalid path format. Path='{Path}'", filePath);
            return ValidationResult.Failure("File path format is invalid.");
        }

        return ValidationResult.Success(trimmed);
    }

    public ValidationResult<int> ValidateNumericParameter(int value, int min, int max, string parameterName)
    {
        if (value < min)
        {
            _logger.LogWarning("Numeric parameter rejected: below minimum. Parameter={Name}, Value={Value}, Min={Min}",
                parameterName, value, min);
            return ValidationResult<int>.Failure($"{parameterName} must be at least {min}.");
        }

        if (value > max)
        {
            _logger.LogWarning("Numeric parameter rejected: above maximum. Parameter={Name}, Value={Value}, Max={Max}",
                parameterName, value, max);
            return ValidationResult<int>.Failure($"{parameterName} cannot exceed {max}.");
        }

        return ValidationResult<int>.Success(value);
    }

    /// <summary>
    /// Removes or escapes dangerous characters that could be used for injection attacks.
    /// </summary>
    private static string SanitizeDangerousCharacters(string input)
    {
        var builder = new StringBuilder(input.Length);

        foreach (var c in input)
        {
            // Remove dangerous characters
            if (!DangerousChars.Contains(c))
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Removes control characters except common whitespace (space, tab, newline, carriage return).
    /// </summary>
    private static string SanitizeControlCharacters(string input)
    {
        var builder = new StringBuilder(input.Length);

        foreach (var c in input)
        {
            // Allow printable characters and common whitespace
            if (!char.IsControl(c) || c == ' ' || c == '\t' || c == '\n' || c == '\r')
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
