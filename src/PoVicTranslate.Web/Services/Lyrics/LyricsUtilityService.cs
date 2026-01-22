namespace PoVicTranslate.Web.Services.Lyrics;

/// <summary>
/// Utility service for lyrics processing.
/// </summary>
public sealed class LyricsUtilityService : ILyricsUtilityService
{
    /// <inheritdoc />
    public string TruncateToWordCount(string text, int maxWords)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var words = text.Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

        if (words.Length <= maxWords)
        {
            return text;
        }

        return string.Join(" ", words.Take(maxWords)) + "...";
    }
}
