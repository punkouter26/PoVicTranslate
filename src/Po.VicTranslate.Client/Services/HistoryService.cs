using Po.VicTranslate.Client.Models;
using System.Text.Json;
using Microsoft.JSInterop;

namespace Po.VicTranslate.Client.Services;

public class HistoryService
{
    private const string StorageKey = "translation-history";
    private const int MaxHistoryItems = 5;
    private const int MaxAgeDays = 7;

    private readonly IJSRuntime _jsRuntime;
    private List<TranslationHistoryItem> _cache = new();
    private bool _isInitialized;

    public event Action? OnChange;

    public HistoryService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<List<TranslationHistoryItem>> GetHistoryAsync()
    {
        if (!_isInitialized)
        {
            await LoadFromStorageAsync();
        }
        return _cache;
    }

    public async Task AddItemAsync(string inputText, string translatedText)
    {
        if (!_isInitialized)
        {
            await LoadFromStorageAsync();
        }

        var item = new TranslationHistoryItem
        {
            InputText = inputText,
            TranslatedText = translatedText,
            Timestamp = DateTime.Now
        };

        // Remove any existing similar entry
        _cache.RemoveAll(x => x.InputText == inputText);

        // Add to beginning
        _cache.Insert(0, item);

        // Keep only max items
        if (_cache.Count > MaxHistoryItems)
        {
            _cache = _cache.Take(MaxHistoryItems).ToList();
        }

        await SaveToStorageAsync();
        OnChange?.Invoke();
    }

    public async Task RemoveItemAsync(string id)
    {
        if (!_isInitialized)
        {
            await LoadFromStorageAsync();
        }

        _cache.RemoveAll(x => x.Id == id);
        await SaveToStorageAsync();
        OnChange?.Invoke();
    }

    public async Task ClearHistoryAsync()
    {
        _cache.Clear();
        await SaveToStorageAsync();
        OnChange?.Invoke();
    }

    private async Task LoadFromStorageAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);

            if (!string.IsNullOrEmpty(json))
            {
                var items = JsonSerializer.Deserialize<List<TranslationHistoryItem>>(json) ?? new();

                // Remove old items
                var cutoffDate = DateTime.Now.AddDays(-MaxAgeDays);
                _cache = items.Where(x => x.Timestamp > cutoffDate).ToList();

                // If we removed any, save the cleaned list
                if (_cache.Count != items.Count)
                {
                    await SaveToStorageAsync();
                }
            }
            else
            {
                _cache = new List<TranslationHistoryItem>();
            }
        }
        catch (Exception)
        {
            _cache = new List<TranslationHistoryItem>();
        }

        _isInitialized = true;
    }

    private async Task SaveToStorageAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_cache);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
        catch (Exception)
        {
            // Silently fail - localStorage might be disabled
        }
    }
}
