namespace PoVicTranslate.Web.Services;

/// <summary>
/// Interface for audio synthesis service.
/// </summary>
public interface IAudioSynthesisService
{
    /// <summary>
    /// Synthesizes speech audio from text.
    /// </summary>
    Task<byte[]> SynthesizeSpeechAsync(string text);
}
