namespace VictorianTranslator.Services;

public interface ITextToSpeechService
{
    Task SpeakTextAsync(string text);
    Task<byte[]> GetAudioBytesAsync(string text);
}
