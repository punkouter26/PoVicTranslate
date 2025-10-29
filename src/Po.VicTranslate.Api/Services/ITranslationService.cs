namespace Po.VicTranslate.Api.Services;

public interface ITranslationService
{
    Task<string> TranslateToVictorianEnglishAsync(string modernText);
}
