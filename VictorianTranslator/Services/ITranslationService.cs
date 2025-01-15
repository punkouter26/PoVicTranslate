namespace VictorianTranslator.Services;

public interface ITranslationService
{
    Task<string> TranslateToVictorianEnglishAsync(string modernText);
}
