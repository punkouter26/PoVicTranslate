namespace Po.VicTranslate.Api.Services.Lyrics;

/// <summary>
/// Service for common lyrics-related utility operations
/// </summary>
public class LyricsUtilityService : ILyricsUtilityService
{
    /// <summary>
    /// Limits text to a maximum number of words, adding ellipsis if truncated
    /// </summary>
    /// <param name="text">The text to limit</param>
    /// <param name="maxWords">Maximum number of words to include</param>
    /// <returns>The limited text with ellipsis if truncated</returns>
    public string LimitWords(string text, int maxWords)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text ?? string.Empty;

        if (maxWords <= 0)
            return string.Empty;

        var words = text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length <= maxWords)
            return text;

        return string.Join(" ", words.Take(maxWords)) + "...";
    }
}
