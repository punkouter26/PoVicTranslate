# Azure OpenAI Content Filtering Guidance

## Issue
Azure OpenAI's Responsible AI content filters are blocking translations of rap lyrics containing profanity and violent themes.

## Current Behavior
- Content with profanity/violence triggers Azure OpenAI's content filter
- Response returns with `FinishReason = ContentFilter`
- User sees: "Alas, the content could not be translated due to safety restrictions"

## Solutions (Ranked by Ease of Implementation)

### Option 1: Adjust Content Filter Settings in Azure Portal ⭐ RECOMMENDED
**Best for**: Production apps that need to handle explicit content responsibly

**Steps**:
1. Go to [Azure AI Foundry Portal](https://ai.azure.com/)
2. Navigate to your Azure OpenAI resource: `povictranslate-openai`
3. Go to **Content filters** section
4. Create or modify a content filtering configuration:
   - **For Prompts**: Set severity to "Medium, High" or "High only" (allows low-severity content)
   - **For Completions**: Set severity to "Medium, High" or "High only"
5. Associate this configuration with your `gpt-4o` deployment
6. Save and test

**Pros**:
- No code changes required
- Granular control over what's blocked
- Complies with Azure's Responsible AI guidelines
- Can still block truly harmful content (high severity)

**Cons**:
- Requires Azure portal access
- May require justification for modified filters

**Documentation**: 
- [Configure Content Filters](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/how-to/content-filters)

---

### Option 2: Pre-process Content (Content Sanitization)
**Best for**: Apps that want to keep strict filters but handle edge cases

**Implementation**:
```csharp
// In TranslationService.cs before calling Azure OpenAI
private string SanitizeContent(string text)
{
    // Replace common profanity with Victorian-appropriate euphemisms
    var sanitized = text
        .Replace("fuck", "forsooth")
        .Replace("shit", "rubbish")
        .Replace("bitch", "scoundrel")
        .Replace("damn", "confound")
        // ... more replacements
        ;
    
    return sanitized;
}
```

**Pros**:
- Works with default filters
- No Azure portal changes
- Can be version controlled

**Cons**:
- Incomplete solution (can't catch everything)
- May miss context/nuance
- Requires maintenance of word list

---

### Option 3: Request Modified Content Filters (Full Control)
**Best for**: Enterprise apps with specific content handling requirements

**Steps**:
1. Apply for modified content filtering via: [Azure OpenAI Limited Access Review](https://ncv.microsoft.com/uEfCgnITdR)
2. Provide business justification
3. Wait for approval (can take several weeks)
4. Once approved, configure filters to "Annotate only" mode
5. Implement custom content moderation in your application

**Pros**:
- Complete control over filtering
- Get annotations without blocking
- Can implement custom business logic

**Cons**:
- Requires approval process
- Takes time to get access
- Must implement your own content moderation
- Higher responsibility for appropriate use

---

### Option 4: User Warning + Retry Logic
**Best for**: Quick fix while waiting for other solutions

**Implementation**:
```csharp
// Show user a warning before translation
"Note: Some content may be blocked by content filters. Translations of explicit 
lyrics may not be available. Consider selecting songs with cleaner language."
```

**Pros**:
- Immediate fix
- Sets user expectations
- No code/config changes

**Cons**:
- Doesn't solve the problem
- Poor user experience
- Limits app functionality

---

## Recommended Approach for PoVicTranslate

**Short-term**: 
1. ✅ Add better error message (already implemented)
2. Adjust content filter to "Medium, High" severity in Azure portal
3. Add user guidance about content restrictions

**Long-term**:
1. Apply for modified content filters if this is a recurring issue
2. Implement hybrid approach:
   - Use "High only" filter for prompts
   - Add content warning labels on explicit songs
   - Provide feedback to users when content is filtered

## Testing
After adjusting filters, test with the "Bastards" song again. The content should pass through with less restrictive filters.

## References
- [Azure OpenAI Content Filtering](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/concepts/content-filter)
- [Responsible AI Overview](https://learn.microsoft.com/en-us/azure/ai-foundry/responsible-ai/openai/overview)
- [Content Filter Configurability](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/concepts/content-filter-configurability)
