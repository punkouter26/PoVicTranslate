namespace VictorianTranslator.Configuration;

public class ApiSettings
{
    public string AzureOpenAIApiKey { get; set; } = string.Empty;
    public string AzureOpenAIEndpoint { get; set; } = string.Empty;
    public string AzureOpenAIDeploymentName { get; set; } = string.Empty;

    // Azure Speech Service Settings
    public string AzureSpeechSubscriptionKey { get; set; } = string.Empty;
    public string AzureSpeechRegion { get; set; } = string.Empty;
}
