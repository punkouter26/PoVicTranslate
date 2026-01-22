namespace PoVicTranslate.Web.Client.Services;

/// <summary>
/// Client-side history service for tracking translation history.
/// Uses in-memory storage (resets on page refresh).
/// </summary>
public sealed class HistoryService
{
    private readonly List<HistoryEntry> _entries = [];
    private const int MaxEntries = 50;

    public IReadOnlyList<HistoryEntry> Entries => _entries.AsReadOnly();

    public void AddEntry(string originalText, string translatedText)
    {
        var entry = new HistoryEntry
        {
            Id = Guid.NewGuid(),
            OriginalText = originalText,
            TranslatedText = translatedText,
            Timestamp = DateTimeOffset.UtcNow
        };

        _entries.Insert(0, entry);

        // Limit history size
        if (_entries.Count > MaxEntries)
        {
            _entries.RemoveRange(MaxEntries, _entries.Count - MaxEntries);
        }
    }

    public void ClearHistory() => _entries.Clear();
}

/// <summary>
/// Represents a single translation history entry.
/// </summary>
public sealed record HistoryEntry
{
    public Guid Id { get; init; }
    public required string OriginalText { get; init; }
    public required string TranslatedText { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
