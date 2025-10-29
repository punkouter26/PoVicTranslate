namespace Po.VicTranslate.Api.Services;

/// <summary>
/// Service for synthesizing speech audio from text using Azure Speech Services
/// </summary>
public interface IAudioSynthesisService
{
    Task<byte[]> SynthesizeSpeechAsync(string text);
}
