using System.Net.Http;
using System.Net.Http.Json; // Added for PostAsJsonAsync
using System.Threading.Tasks;

namespace Po.VicTranslate.Client.Services;

public class ClientSpeechService
{
    private readonly HttpClient _httpClient;

    public ClientSpeechService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> SynthesizeSpeechAsync(string text)
    {
        var response = await _httpClient.PostAsJsonAsync("Speech/synthesize", text);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Speech service returned status {response.StatusCode}: {errorMessage}");
        }

        return await response.Content.ReadAsByteArrayAsync();
    }
}
