namespace Po.VicTranslate.Client.Models;

public class TranslationResponse
{
    public required string TranslatedText { get; set; }
    public byte[]? AudioData { get; set; }
}
