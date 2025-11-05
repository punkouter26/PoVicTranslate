namespace Po.VicTranslate.Api.Services.Translation;

/// <summary>
/// Builds system and user prompts for Victorian English translation.
/// Encapsulates prompt engineering logic for improved maintainability.
/// </summary>
public class VictorianPromptBuilder
{
    /// <summary>
    /// Builds the system and user prompts for translating modern text to Victorian English.
    /// </summary>
    /// <param name="modernText">The modern English text to translate</param>
    /// <returns>A tuple containing the system prompt and user prompt</returns>
    public (string SystemPrompt, string UserPrompt) BuildPrompts(string modernText)
    {
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

        var userPrompt = $"Pray, render the following modern text into the Queen's English of the Victorian age: '{modernText}'";

        return (systemPrompt, userPrompt);
    }
}
