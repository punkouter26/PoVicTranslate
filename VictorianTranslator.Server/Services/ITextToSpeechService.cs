namespace VictorianTranslator.Services;

public interface ITextToSpeechService
{
    Task SpeakTextAsync(string text);
    // Removed GetAudioBytesAsync as it's not used by the Azure Speech SDK implementation
}
