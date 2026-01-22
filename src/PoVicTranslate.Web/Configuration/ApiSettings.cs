namespace PoVicTranslate.Web.Configuration;

/// <summary>
/// Configuration settings for Azure services.
/// </summary>
public sealed class ApiSettings
{
    public string AzureOpenAIApiKey { get; set; } = string.Empty;
    public string AzureOpenAIEndpoint { get; set; } = string.Empty;
    public string AzureOpenAIDeploymentName { get; set; } = string.Empty;
    public string AzureSpeechSubscriptionKey { get; set; } = string.Empty;
    public string AzureSpeechRegion { get; set; } = string.Empty;
}
