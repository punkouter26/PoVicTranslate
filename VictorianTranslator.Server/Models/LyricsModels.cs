namespace VictorianTranslator.Server.Models;

public class LyricsSong
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Artist { get; set; }
    public required string Album { get; set; }
    public int? Year { get; set; }
    public required string Genre { get; set; }
    public required string Content { get; set; }
    public int WordCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Description { get; set; }
}

public class LyricsCollection
{
    public required string Version { get; set; }
    public DateTime GeneratedAt { get; set; }
    public int TotalSongs { get; set; }
    public Dictionary<string, string> Artists { get; set; } = new();
    public Dictionary<string, string> Albums { get; set; } = new();
    public List<LyricsSong> Songs { get; set; } = new();
}

public class LyricsSearchResult
{
    public required LyricsSong Song { get; set; }
    public double RelevanceScore { get; set; }
    public List<string> MatchedTerms { get; set; } = new();
}
