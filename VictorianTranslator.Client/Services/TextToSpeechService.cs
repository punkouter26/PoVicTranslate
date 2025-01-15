using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using VictorianTranslator.Configuration;

namespace VictorianTranslator.Client.Services;

public class TextToSpeechService : ITextToSpeechService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _voiceId;
    private readonly IJSRuntime _jsRuntime;
    private const string ELEVEN_LABS_API_URL = "https://api.elevenlabs.io/v1/text-to-speech";

    public TextToSpeechService(HttpClient httpClient, IOptions<ApiSettings> apiSettings, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _apiKey = apiSettings.Value.ElevenLabsApiKey;
        _voiceId = apiSettings.Value.ElevenLabsVoiceId;
    }

    public async Task<byte[]> GetAudioBytesAsync(string text)
    {
        var requestBody = new
        {
            text = text,
            model_id = "eleven_monolingual_v1",
            voice_settings = new
            {
                stability = 0.5,
                similarity_boost = 0.75
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{ELEVEN_LABS_API_URL}/{_voiceId}")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            )
        };

        request.Headers.Add("xi-api-key", _apiKey);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"ElevenLabs API error: {errorContent}");
        }

        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task SpeakTextAsync(string text)
    {
        var audioBytes = await GetAudioBytesAsync(text);
        
        // Convert the audio bytes to a base64 string
        var audioBase64 = Convert.ToBase64String(audioBytes);
        
        // Create an audio element and play it using JavaScript interop
        await _jsRuntime.InvokeVoidAsync("playAudio", audioBase64);
    }
}
