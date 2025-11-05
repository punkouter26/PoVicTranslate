namespace Po.VicTranslate.Client.Models;

public class TranslationHistoryItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string InputText { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    
    public string InputPreview => InputText.Length > 50 
        ? InputText.Substring(0, 47) + "..." 
        : InputText;
    
    public string TranslatedPreview => TranslatedText.Length > 50 
        ? TranslatedText.Substring(0, 47) + "..." 
        : TranslatedText;
}
