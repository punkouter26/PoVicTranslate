using Microsoft.Extensions.Options;
using Po.VicTranslate.Api.Configuration;
using Microsoft.Extensions.Logging;
using Po.VicTranslate.Api.Services.Validation;
using System.Net.Http.Headers;
using System.Text;

namespace Po.VicTranslate.Api.Services;

/// <summary>
/// Azure Speech Services implementation for audio synthesis.
/// Uses the REST API instead of the SDK to avoid native library dependencies.
/// This works on all platforms including Azure App Service Linux.
/// </summary>
public class AudioSynthesisService : IAudioSynthesisService
{
    private readonly ApiSettings _settings;
    private readonly ISpeechConfigValidator _configValidator;
    private readonly ILogger<AudioSynthesisService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    private const string VoiceName = "en-GB-RyanNeural";

    public AudioSynthesisService(
        IOptions<ApiSettings> apiSettings,
        ISpeechConfigValidator configValidator,
        IHttpClientFactory httpClientFactory,
        ILogger<AudioSynthesisService> logger)
    {
        ArgumentNullException.ThrowIfNull(apiSettings);
        ArgumentNullException.ThrowIfNull(configValidator);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        _settings = apiSettings.Value;
        _configValidator = configValidator;
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        _logger.LogInformation("AudioSynthesisService created using REST API (no native SDK required)");
    }

    private async Task<string> GetAccessTokenAsync()
    {
        // Return cached token if still valid (with 1 minute buffer)
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-1))
        {
            return _accessToken;
        }

        var tokenEndpoint = $"https://{_settings.AzureSpeechRegion}.api.cognitive.microsoft.com/sts/v1.0/issueToken";

        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.AzureSpeechSubscriptionKey);

        _logger.LogInformation("Fetching new access token from {Endpoint}", tokenEndpoint);

        var response = await client.PostAsync(tokenEndpoint, new StringContent(string.Empty));
        response.EnsureSuccessStatusCode();

        _accessToken = await response.Content.ReadAsStringAsync();
        _tokenExpiry = DateTime.UtcNow.AddMinutes(9); // Token valid for 10 minutes, refresh at 9

        _logger.LogInformation("Access token obtained successfully");
        return _accessToken;
    }

    public async Task<byte[]> SynthesizeSpeechAsync(string text)
    {
        // Validate configuration
        if (!_configValidator.IsValid(_settings))
        {
            var error = _configValidator.GetValidationError(_settings);
            _logger.LogError("Azure Speech settings validation failed: {Error}", error);
            throw new InvalidOperationException($"Azure Speech settings are not configured: {error}");
        }

        _logger.LogInformation("Starting speech synthesis for text length: {Length}", text.Length);

        var accessToken = await GetAccessTokenAsync();

        var ttsEndpoint = $"https://{_settings.AzureSpeechRegion}.tts.speech.microsoft.com/cognitiveservices/v1";

        // Build SSML
        var ssml = $@"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-GB'>
            <voice name='{VoiceName}'>
                {System.Security.SecurityElement.Escape(text)}
            </voice>
        </speak>";

        using var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, ttsEndpoint);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-Microsoft-OutputFormat", "audio-16khz-32kbitrate-mono-mp3");
        request.Headers.Add("User-Agent", "PoVicTranslate");

        request.Content = new StringContent(ssml, Encoding.UTF8, "application/ssml+xml");

        _logger.LogInformation("Sending TTS request to {Endpoint} with voice {Voice}", ttsEndpoint, VoiceName);

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Speech synthesis failed: {StatusCode} - {Error}", response.StatusCode, errorContent);

            var errorMessage = response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Authentication failed. Please verify your Azure Speech Service subscription key.",
                System.Net.HttpStatusCode.BadRequest => "Bad request. The voice or configuration may not be supported.",
                System.Net.HttpStatusCode.TooManyRequests => "Too many requests. Please try again later.",
                System.Net.HttpStatusCode.Forbidden => "Access forbidden. Please check your subscription permissions.",
                _ => $"Speech synthesis failed: {response.StatusCode}. Details: {errorContent}"
            };

            throw new Exception(errorMessage);
        }

        var audioData = await response.Content.ReadAsByteArrayAsync();
        _logger.LogInformation("Speech synthesis completed successfully. Audio size: {Size} bytes", audioData.Length);

        return audioData;
    }
}
