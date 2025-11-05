using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Po.VicTranslate.Client.Services;

public class ClientLyricsService
{
    private readonly HttpClient _httpClient;

    public ClientLyricsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<string>> GetAvailableSongsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<string>>("api/Lyrics") ?? new List<string>();
    }

    public async Task<string> GetLyricsAsync(string songFileName)
    {
        return await _httpClient.GetStringAsync($"api/Lyrics/{songFileName}");
    }
}
