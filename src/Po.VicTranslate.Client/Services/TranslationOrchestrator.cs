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
    Task<bool> CopyToClipboardAsync(string text);
}

public class TranslationOrchestrator : ITranslationOrchestrator
{
    private readonly ClientTranslationService _translationService;
    private readonly ClientLyricsService _lyricsService;
    private readonly IJSRuntime _jsRuntime;

    public TranslationOrchestrator(
        ClientTranslationService translationService,
        ClientLyricsService lyricsService,
        IJSRuntime jsRuntime)
    {
        _translationService = translationService;
        _lyricsService = lyricsService;
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
            var response = await _translationService.TranslateText(viewModel.InputText);
            viewModel.TranslatedText = response.TranslatedText ?? string.Empty;

            // Automatically play audio if available
            if (response.AudioData != null && response.AudioData.Length > 0)
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("playAudio", response.AudioData);
                }
                catch (Exception ex)
                {
                    // Don't fail the translation if audio playback fails
                    System.Diagnostics.Debug.WriteLine($"Audio playback failed: {ex.Message}");
                }
            }

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
