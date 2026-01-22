namespace PoVicTranslate.Web.Services.Lyrics;

/// <summary>
/// Interface for lyrics utility service.
/// </summary>
public interface ILyricsUtilityService
{
    /// <summary>
    /// Truncates text to maximum word count.
    /// </summary>
    string TruncateToWordCount(string text, int maxWords);
}
