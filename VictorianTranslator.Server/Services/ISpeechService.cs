namespace VictorianTranslator.Server.Services
{
    public interface ISpeechService
    {
        Task<byte[]> SynthesizeSpeechAsync(string text);
    }
}
