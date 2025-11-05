namespace Po.VicTranslate.Api.Services.BrowserLog;

/// <summary>
/// Factory for selecting the appropriate browser log strategy based on log type.
/// Implements the Factory Pattern to decouple strategy creation from usage.
/// </summary>
public class BrowserLogStrategyFactory
{
    private readonly IEnumerable<IBrowserLogStrategy> _strategies;

    /// <summary>
    /// Initializes a new instance of the BrowserLogStrategyFactory.
    /// </summary>
    /// <param name="strategies">Collection of all available browser log strategies</param>
    public BrowserLogStrategyFactory(IEnumerable<IBrowserLogStrategy> strategies)
    {
        _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
    }

    /// <summary>
    /// Gets the appropriate strategy for the given log type.
    /// Returns UnknownLogStrategy if no specific strategy matches.
    /// </summary>
    /// <param name="logType">The type of browser log</param>
    /// <returns>The matching strategy or fallback to UnknownLogStrategy</returns>
    public IBrowserLogStrategy GetStrategy(string? logType)
    {
        // Find first matching strategy (excluding UnknownLogStrategy)
        var strategy = _strategies
            .Where(s => s.GetType() != typeof(UnknownLogStrategy))
            .FirstOrDefault(s => s.CanHandle(logType));

        // Fallback to UnknownLogStrategy if no match found
        return strategy ?? _strategies.OfType<UnknownLogStrategy>().First();
    }
}
