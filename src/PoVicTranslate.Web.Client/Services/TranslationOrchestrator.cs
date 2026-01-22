using PoVicTranslate.Web.Client.ViewModels;

namespace PoVicTranslate.Web.Client.Services;

/// <summary>
/// Orchestrates translation operations including song loading and translation.
/// Implements the BFF (Backend For Frontend) pattern by calling server endpoints.
/// </summary>
public sealed class TranslationOrchestrator(
    ClientLyricsService lyricsService,
    ClientTranslationService translationService,
    ClientSpeechService speechService,
    HistoryService historyService) : ITranslationOrchestrator
{
    public async Task InitializeAsync(TranslationViewModel viewModel)
    {
        viewModel.IsLoading = true;
        try
        {
            var titles = await lyricsService.GetSongTitlesAsync();
            viewModel.AvailableSongs = titles;
        }
        catch
        {
            viewModel.ErrorMessage = "Failed to load song list";
        }
        finally
        {
            viewModel.IsLoading = false;
        }
    }

    public async Task LoadSongAsync(TranslationViewModel viewModel, string songTitle)
    {
        viewModel.IsLoading = true;
        viewModel.ErrorMessage = null;

        try
        {
            var lyrics = await lyricsService.GetLyricsAsync(songTitle);
            viewModel.InputText = lyrics ?? string.Empty;
        }
        catch
        {
            viewModel.ErrorMessage = $"Failed to load lyrics for '{songTitle}'";
        }
        finally
        {
            viewModel.IsLoading = false;
        }
    }

    public async Task LoadRandomSongAsync(TranslationViewModel viewModel)
    {
        viewModel.IsLoading = true;
        viewModel.ErrorMessage = null;

        try
        {
            var lyrics = await lyricsService.GetRandomLyricsAsync();
            viewModel.InputText = lyrics ?? string.Empty;
        }
        catch
        {
            viewModel.ErrorMessage = "Failed to load random song";
        }
        finally
        {
            viewModel.IsLoading = false;
        }
    }

    public async Task TranslateTextAsync(TranslationViewModel viewModel)
    {
        viewModel.IsTranslating = true;
        viewModel.ErrorMessage = null;

        try
        {
            var result = await translationService.TranslateAsync(viewModel.InputText);
            viewModel.TranslatedText = result?.TranslatedText ?? string.Empty;

            // Add to history
            historyService.AddEntry(viewModel.InputText, viewModel.TranslatedText);

            // Synthesize and play speech
            if (!string.IsNullOrEmpty(viewModel.TranslatedText))
            {
                var audioBytes = await speechService.SynthesizeSpeechAsync(viewModel.TranslatedText);
                if (audioBytes != null && audioBytes.Length > 0)
                {
                    viewModel.AudioBytes = audioBytes;
                }
            }
        }
        catch (HttpRequestException ex)
        {
            viewModel.ErrorMessage = $"Translation service error: {ex.Message}";
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = $"Unexpected error: {ex.Message}";
        }
        finally
        {
            viewModel.IsTranslating = false;
        }
    }
}
