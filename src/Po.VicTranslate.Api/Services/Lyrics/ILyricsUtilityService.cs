namespace Po.VicTranslate.Api.Services.Lyrics;

/// <summary>
/// Service for common lyrics-related utility operations
/// </summary>
public interface ILyricsUtilityService
{
    /// <summary>
    /// Limits text to a maximum number of words, adding ellipsis if truncated
    /// </summary>
    /// <param name="text">The text to limit</param>
    /// <param name="maxWords">Maximum number of words to include</param>
    /// <returns>The limited text with ellipsis if truncated</returns>
    string LimitWords(string text, int maxWords);
}
