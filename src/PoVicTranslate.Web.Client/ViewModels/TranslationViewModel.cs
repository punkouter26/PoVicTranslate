namespace PoVicTranslate.Web.Client.ViewModels;

/// <summary>
/// ViewModel for the translation page. Holds UI state and data.
/// </summary>
public sealed class TranslationViewModel
{
    public string InputText { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public bool IsLoading { get; set; }
    public bool IsTranslating { get; set; }
    public List<string> AvailableSongs { get; set; } = [];

    public bool CanTranslate => !IsTranslating && !string.IsNullOrWhiteSpace(InputText);
}
