using System.Net.Http.Json;

namespace PoVicTranslate.Web.Client.Services;

/// <summary>
/// Client service for communicating with the server's translation endpoints.
/// </summary>
public sealed class ClientTranslationService(HttpClient httpClient)
{
    /// <summary>
    /// Translates text using the server-side translation API.
    /// </summary>
    public async Task<TranslationResult?> TranslateAsync(string text, CancellationToken cancellationToken = default)
    {
        var request = new TranslationRequest(text);
        var response = await httpClient.PostAsJsonAsync("/api/translation", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TranslationResult>(cancellationToken);
    }
}

/// <summary>
/// Request DTO for translation.
/// </summary>
public sealed record TranslationRequest(string Text);

/// <summary>
/// Response DTO from translation endpoint.
/// </summary>
public sealed record TranslationResult(
    string OriginalText,
    string TranslatedText,
    byte[]? AudioData);
