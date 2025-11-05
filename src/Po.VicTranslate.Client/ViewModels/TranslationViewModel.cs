using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Po.VicTranslate.Client.ViewModels;

/// <summary>
/// View model for managing translation component state
/// </summary>
public class TranslationViewModel : INotifyPropertyChanged
{
    private string _inputText = string.Empty;
    private string _translatedText = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isTranslating;
    private bool _isSpeaking;
    private bool _isLoading;
    private List<string> _availableSongs = new();
    private bool _isEditMode;
    private string _editedText = string.Empty;
    private string _originalTranslatedText = string.Empty;

    public string InputText
    {
        get => _inputText;
        set => SetProperty(ref _inputText, value);
    }

    public string TranslatedText
    {
        get => _translatedText;
        set => SetProperty(ref _translatedText, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsTranslating
    {
        get => _isTranslating;
        set => SetProperty(ref _isTranslating, value);
    }

    public bool IsSpeaking
    {
        get => _isSpeaking;
        set => SetProperty(ref _isSpeaking, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public List<string> AvailableSongs
    {
        get => _availableSongs;
        set => SetProperty(ref _availableSongs, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public string EditedText
    {
        get => _editedText;
        set => SetProperty(ref _editedText, value);
    }

    public string OriginalTranslatedText
    {
        get => _originalTranslatedText;
        set => SetProperty(ref _originalTranslatedText, value);
    }

    public int WordCount => CalculateWordCount();

    public bool CanTranslate => !IsTranslating && !string.IsNullOrWhiteSpace(InputText) && WordCount <= 200;

    public bool CanSpeak => !IsSpeaking && !string.IsNullOrWhiteSpace(TranslatedText);

    private int CalculateWordCount()
    {
        if (string.IsNullOrWhiteSpace(InputText))
        {
            return 0;
        }

        return InputText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public void ClearError()
    {
        ErrorMessage = string.Empty;
    }

    public void ClearTranslation()
    {
        TranslatedText = string.Empty;
    }

    public void EnterEditMode()
    {
        if (string.IsNullOrWhiteSpace(TranslatedText))
        {
            return;
        }

        OriginalTranslatedText = TranslatedText;
        EditedText = TranslatedText;
        IsEditMode = true;
    }

    public void SaveEdit()
    {
        if (IsEditMode)
        {
            TranslatedText = EditedText;
            ExitEditMode();
        }
    }

    public void CancelEdit()
    {
        if (IsEditMode)
        {
            TranslatedText = OriginalTranslatedText;
            ExitEditMode();
        }
    }

    public void ExitEditMode()
    {
        IsEditMode = false;
        EditedText = string.Empty;
        OriginalTranslatedText = string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return false;
        }

        backingStore = value;
        OnPropertyChanged(propertyName);

        // Notify dependent properties
        if (propertyName == nameof(InputText))
        {
            OnPropertyChanged(nameof(WordCount));
            OnPropertyChanged(nameof(CanTranslate));
        }
        else if (propertyName == nameof(IsTranslating))
        {
            OnPropertyChanged(nameof(CanTranslate));
        }
        else if (propertyName == nameof(TranslatedText) || propertyName == nameof(IsSpeaking))
        {
            OnPropertyChanged(nameof(CanSpeak));
        }

        return true;
    }
}
