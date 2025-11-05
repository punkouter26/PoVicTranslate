using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Po.VicTranslate.Client.Models;

namespace Po.VicTranslate.Client.Services;

public class ClientTranslationService
{
    private readonly HttpClient _httpClient;

    public ClientTranslationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TranslationResponse> TranslateText(string text)
    {
        var request = new TranslationRequest { Text = text };
        var response = await _httpClient.PostAsJsonAsync("api/Translation", request);
        response.EnsureSuccessStatusCode();
        var translationResponse = await response.Content.ReadFromJsonAsync<TranslationResponse>();
        return translationResponse ?? new TranslationResponse { TranslatedText = string.Empty };
    }
}
