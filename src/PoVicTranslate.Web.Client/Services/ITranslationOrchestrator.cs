namespace PoVicTranslate.Web.Client.Services;

/// <summary>
/// Orchestrates translation operations including song loading and translation.
/// </summary>
public interface ITranslationOrchestrator
{
    /// <summary>
    /// Initializes the view model with available songs.
    /// </summary>
    Task InitializeAsync(ViewModels.TranslationViewModel viewModel);

    /// <summary>
    /// Loads a specific song's lyrics into the input text.
    /// </summary>
    Task LoadSongAsync(ViewModels.TranslationViewModel viewModel, string songTitle);

    /// <summary>
    /// Loads a random song's lyrics into the input text.
    /// </summary>
    Task LoadRandomSongAsync(ViewModels.TranslationViewModel viewModel);

    /// <summary>
    /// Translates the input text and updates the view model with the result.
    /// </summary>
    Task TranslateTextAsync(ViewModels.TranslationViewModel viewModel);
}
