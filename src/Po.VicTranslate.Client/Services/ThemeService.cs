using Microsoft.JSInterop;

namespace Po.VicTranslate.Client.Services;

public class ThemeService
{
    private const string StorageKey = "theme-preference";
    private const string DefaultTheme = "dark";
    
    private readonly IJSRuntime _jsRuntime;
    private string _currentTheme = DefaultTheme;
    private bool _isInitialized;

    public event Action? OnThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public string CurrentTheme => _currentTheme;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            // Try to get saved preference
            var saved = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
            
            if (!string.IsNullOrEmpty(saved) && (saved == "light" || saved == "dark"))
            {
                _currentTheme = saved;
            }
            else
            {
                // Detect system preference
                var prefersDark = await _jsRuntime.InvokeAsync<bool>("matchMedia", "(prefers-color-scheme: dark)");
                _currentTheme = prefersDark ? "dark" : "light";
            }

            await ApplyThemeAsync(_currentTheme);
        }
        catch (Exception)
        {
            _currentTheme = DefaultTheme;
            await ApplyThemeAsync(_currentTheme);
        }

        _isInitialized = true;
    }

    public async Task ToggleThemeAsync()
    {
        var newTheme = _currentTheme == "dark" ? "light" : "dark";
        await SetThemeAsync(newTheme);
    }

    public async Task SetThemeAsync(string theme)
    {
        if (theme != "light" && theme != "dark") return;

        _currentTheme = theme;
        await ApplyThemeAsync(theme);
        await SaveThemeAsync(theme);
        OnThemeChanged?.Invoke();
    }

    private async Task ApplyThemeAsync(string theme)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('data-theme', '{theme}')");
        }
        catch (Exception)
        {
            // Silently fail
        }
    }

    private async Task SaveThemeAsync(string theme)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, theme);
        }
        catch (Exception)
        {
            // Silently fail - localStorage might be disabled
        }
    }
}
