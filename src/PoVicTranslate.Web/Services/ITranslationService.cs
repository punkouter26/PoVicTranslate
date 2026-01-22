namespace PoVicTranslate.Web.Services;

/// <summary>
/// Interface for translation service.
/// </summary>
public interface ITranslationService
{
    /// <summary>
    /// Translates modern English text to Victorian-era English.
    /// </summary>
    Task<string> TranslateToVictorianEnglishAsync(string modernText);
}
