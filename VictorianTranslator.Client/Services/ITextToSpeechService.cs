namespace VictorianTranslator.Client.Services;

public interface ITextToSpeechService
{
    Task SpeakTextAsync(string text);
    Task<byte[]> GetAudioBytesAsync(string text);
}
