using System.Net.Http.Json;

namespace PoVicTranslate.Web.Client.Services;

/// <summary>
/// Client service for text-to-speech operations.
/// </summary>
public sealed class ClientSpeechService(HttpClient httpClient)
{
    /// <summary>
    /// Synthesizes speech from text and returns the audio bytes.
    /// </summary>
    public async Task<byte[]?> SynthesizeSpeechAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var response = await httpClient.PostAsync($"/api/speech/synthesize?text={Uri.EscapeDataString(text)}", null);
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsByteArrayAsync();
    }
}
