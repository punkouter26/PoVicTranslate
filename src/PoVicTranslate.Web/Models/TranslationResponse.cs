namespace PoVicTranslate.Web.Models;

/// <summary>
/// Response model for translation results.
/// </summary>
public sealed record TranslationResponse(
    string OriginalText,
    string TranslatedText,
    byte[]? AudioData);
