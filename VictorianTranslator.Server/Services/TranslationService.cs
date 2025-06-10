using Azure;
using Azure.AI.OpenAI;
using VictorianTranslator.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging; // Added for logging

namespace VictorianTranslator.Services;

public class TranslationService : ITranslationService
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _deploymentName;
    private readonly ILogger<TranslationService> _logger; // Added logger

    public TranslationService(IOptions<ApiSettings> apiSettings, ILogger<TranslationService> logger)
    {
        _logger = logger; // Assign logger
        var settings = apiSettings.Value;

        if (string.IsNullOrWhiteSpace(settings.AzureOpenAIApiKey) || 
            string.IsNullOrWhiteSpace(settings.AzureOpenAIEndpoint) || 
            string.IsNullOrWhiteSpace(settings.AzureOpenAIDeploymentName))
        {
            _logger.LogError("Azure OpenAI settings (ApiKey, Endpoint, DeploymentName) are not configured properly in appsettings.json.");
            throw new InvalidOperationException("Azure OpenAI settings are not configured.");
        }
        
        _deploymentName = settings.AzureOpenAIDeploymentName;
        
        try
        {
            // Using API Key authentication
            _openAIClient = new OpenAIClient(new Uri(settings.AzureOpenAIEndpoint), new AzureKeyCredential(settings.AzureOpenAIApiKey));
            _logger.LogInformation("OpenAIClient initialized successfully with endpoint {Endpoint}", settings.AzureOpenAIEndpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize OpenAIClient with endpoint {Endpoint}", settings.AzureOpenAIEndpoint);
            throw; // Re-throw the exception after logging
        }
    }

    public async Task<string> TranslateToVictorianEnglishAsync(string modernText)
    {
        _logger.LogInformation("Starting translation for text: '{ModernText}'", modernText);

        // System prompt defining the role and task for the AI model
        var systemPrompt = @"You are a highly skilled translator specializing in converting modern English into authentic Victorian-era English. 
Your task is to translate the user's text while adhering strictly to the following rules:
1. Use formal, elaborate, and sophisticated language characteristic of the Victorian era.
2. Incorporate common Victorian expressions, idioms, and turns of phrase naturally.
3. Maintain a tone of utmost propriety, politeness, and decorum.
4. Employ a richer and more varied vocabulary than modern English.
5. Use appropriate honorifics (e.g., 'Sir', 'Madam', 'Miss') if the context suggests addressing someone, though often the input text won't provide this context.
6. Structure sentences in a more complex manner typical of the period.
7. Avoid modern slang, contractions (use 'do not' instead of 'don't'), and overly casual phrasing.
8. Respond ONLY with the translated Victorian English text. Do not include any preamble, explanation, apologies, or conversational filler. For example, do not say 'Here is the translation:' or 'I trust this meets your requirements.'";

        // User prompt containing the text to be translated
        var userPrompt = $"Pray, render the following modern text into the Queen's English of the Victorian age: '{modernText}'";

        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = _deploymentName, // Use the deployment name from configuration
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt),
            },
            Temperature = 0.7f, // Adjust creativity/determinism
            MaxTokens = 800,    // Adjust based on expected output length
            NucleusSamplingFactor = 0.95f,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        };

        try
        {
            _logger.LogInformation("Sending request to Azure OpenAI deployment '{DeploymentName}'", _deploymentName);
            Response<ChatCompletions> response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
            
            if (response == null || response.Value == null || !response.Value.Choices.Any())
            {
                 _logger.LogWarning("Received null or empty response from Azure OpenAI.");
                 return "Regrettably, the translation could not be procured at this time.";
            }

            ChatResponseMessage responseMessage = response.Value.Choices[0].Message;
            _logger.LogInformation("Successfully received translation from Azure OpenAI.");
            return responseMessage.Content ?? "The translation yielded naught but silence.";

        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure OpenAI API request failed. Status: {Status}, ErrorCode: {ErrorCode}, Message: {Message}", ex.Status, ex.ErrorCode, ex.Message);
            // Consider providing a more user-friendly error or details based on ex.Status / ex.ErrorCode
            throw new Exception($"Translation API error: Failed to communicate with Azure OpenAI. Status: {ex.Status}. Please check logs for details.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during translation.");
            throw new Exception("An unexpected error occurred during the translation process.", ex);
        }
    }
}
