using Po.VicTranslate.Client.Services;
using Po.VicTranslate.Client.ViewModels;
using Microsoft.JSInterop;

namespace Po.VicTranslate.Client.Services;

/// <summary>
/// Orchestrates translation-related operations
/// </summary>
public interface ITranslationOrchestrator
{
    Task<bool> InitializeAsync(TranslationViewModel viewModel);
    Task<bool> LoadSongAsync(TranslationViewModel viewModel, string songFileName);
    Task<bool> LoadRandomSongAsync(TranslationViewModel viewModel);
    Task<bool> TranslateTextAsync(TranslationViewModel viewModel);
    Task<bool> SpeakTextAsync(TranslationViewModel viewModel);
    Task<bool> CopyToClipboardAsync(string text);
}

public class TranslationOrchestrator : ITranslationOrchestrator
{
    private readonly ClientTranslationService _translationService;
    private readonly ClientLyricsService _lyricsService;
    private readonly ClientSpeechService _speechService;
    private readonly IJSRuntime _jsRuntime;

    public TranslationOrchestrator(
        ClientTranslationService translationService,
        ClientLyricsService lyricsService,
        ClientSpeechService speechService,
        IJSRuntime jsRuntime)
    {
        _translationService = translationService;
        _lyricsService = lyricsService;
        _speechService = speechService;
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> InitializeAsync(TranslationViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        try
        {
            viewModel.IsLoading = true;
            viewModel.ClearError();

            var songs = await _lyricsService.GetAvailableSongsAsync();
            viewModel.AvailableSongs = songs;

            return true;
        }
        catch (Exception)
        {
            viewModel.ErrorMessage = "Error loading songs.";
            return false;
        }
        finally
        {
            viewModel.IsLoading = false;
        }
    }

    public async Task<bool> LoadSongAsync(TranslationViewModel viewModel, string songFileName)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        try
        {
            viewModel.IsLoading = true;
            viewModel.ClearError();

            var lyrics = await _lyricsService.GetLyricsAsync(songFileName);
            viewModel.InputText = lyrics;

            return true;
        }
        catch (Exception)
        {
            viewModel.ErrorMessage = "Error loading lyrics.";
            return false;
        }
        finally
        {
            viewModel.IsLoading = false;
        }
    }

    public async Task<bool> LoadRandomSongAsync(TranslationViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        if (!viewModel.AvailableSongs.Any())
        {
            return false;
        }

        var random = new Random();
        var randomSong = viewModel.AvailableSongs[random.Next(viewModel.AvailableSongs.Count)];
        return await LoadSongAsync(viewModel, randomSong);
    }

    public async Task<bool> TranslateTextAsync(TranslationViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        try
        {
            viewModel.ClearError();

            if (string.IsNullOrWhiteSpace(viewModel.InputText))
            {
                viewModel.ErrorMessage = "Please enter some text to translate.";
                return false;
            }

            if (viewModel.WordCount > 200)
            {
                viewModel.ErrorMessage = "Please reduce the text to 200 words or less.";
                return false;
            }

            viewModel.IsTranslating = true;
            var translatedText = await _translationService.TranslateText(viewModel.InputText);
            viewModel.TranslatedText = translatedText ?? string.Empty;

            return true;
        }
        catch (Exception)
        {
            viewModel.ErrorMessage = "Translation error: An unexpected error occurred.";
            viewModel.ClearTranslation();
            return false;
        }
        finally
        {
            viewModel.IsTranslating = false;
        }
    }

    public async Task<bool> SpeakTextAsync(TranslationViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        try
        {
            viewModel.ClearError();

            if (string.IsNullOrWhiteSpace(viewModel.TranslatedText))
            {
                viewModel.ErrorMessage = "No translated text to read.";
                return false;
            }

            viewModel.IsSpeaking = true;
            var audioBytes = await _speechService.SynthesizeSpeechAsync(viewModel.TranslatedText);
            await _jsRuntime.InvokeVoidAsync("playAudio", audioBytes);

            return true;
        }
        catch (HttpRequestException ex)
        {
            viewModel.ErrorMessage = $"Text-to-speech service unavailable: {ex.Message}";
            return false;
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = $"Text-to-speech error: {ex.Message}";
            return false;
        }
        finally
        {
            viewModel.IsSpeaking = false;
        }
    }

    public async Task<bool> CopyToClipboardAsync(string text)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
