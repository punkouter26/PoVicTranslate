using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace Po.VicTranslate.Api.Services.Translation;

/// <summary>
/// Wrapper for Azure OpenAI chat completion operations.
/// Encapsulates API interaction logic and provides a cleaner interface for translation.
/// </summary>
public class AzureOpenAIChatService
{
    private readonly AzureOpenAIClient _client;
    private readonly string _deploymentName;
    private readonly ILogger<AzureOpenAIChatService> _logger;

    public AzureOpenAIChatService(
        AzureOpenAIClient client,
        string deploymentName,
        ILogger<AzureOpenAIChatService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _deploymentName = deploymentName ?? throw new ArgumentNullException(nameof(deploymentName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Completes a chat conversation using Azure OpenAI.
    /// </summary>
    /// <param name="systemPrompt">The system message defining the AI's role and behavior</param>
    /// <param name="userPrompt">The user's message/request</param>
    /// <returns>The AI's response text, or a fallback message if the response is empty</returns>
    public async Task<string> CompleteChatAsync(string systemPrompt, string userPrompt)
    {
        _logger.LogInformation("Sending request to Azure OpenAI deployment '{DeploymentName}'", _deploymentName);

        var chatClient = _client.GetChatClient(_deploymentName);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var chatCompletionOptions = new ChatCompletionOptions
        {
            Temperature = 0.7f,
            MaxOutputTokenCount = 800,
            TopP = 0.95f,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        };

        var response = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);

        _logger.LogInformation("Azure OpenAI response received. HasValue: {HasValue}, ContentCount: {Count}, FinishReason: {FinishReason}", 
            response?.Value != null, 
            response?.Value?.Content?.Count ?? 0,
            response?.Value?.FinishReason);

        // Check if content was filtered by Azure OpenAI's Responsible AI filters
        if (response?.Value?.FinishReason == OpenAI.Chat.ChatFinishReason.ContentFilter)
        {
            _logger.LogWarning("Content was blocked by Azure OpenAI content filtering. " +
                "This typically occurs when input contains profanity, violence, or other restricted content. " +
                "Consider adjusting content filter settings in Azure portal or sanitizing input.");
            
            return "Alas, the content could not be translated due to safety restrictions. " +
                   "Prithee, consider revising the text to remove profanity or explicit themes.";
        }

        if (response?.Value?.Content == null || response.Value.Content.Count == 0)
        {
            _logger.LogWarning("Received null or empty response from Azure OpenAI. Response is null: {IsNull}, Content is null: {ContentNull}", 
                response?.Value == null,
                response?.Value?.Content == null);
            return "Regrettably, the translation could not be procured at this time.";
        }

        var content = response.Value.Content[0].Text;
        _logger.LogInformation("Extracted content from response. Content length: {Length}, IsNull: {IsNull}", 
            content?.Length ?? 0, 
            content == null);
        return content ?? "The translation yielded naught but silence.";
    }
}
