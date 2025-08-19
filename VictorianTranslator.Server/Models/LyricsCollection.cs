using System.Text.Json.Serialization;

namespace VictorianTranslator.Models
{
    public class LyricsCollection
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("generatedAt")]
        public DateTime GeneratedAt { get; set; }

        [JsonPropertyName("totalSongs")]
        public int TotalSongs { get; set; }

        [JsonPropertyName("artists")]
        public Dictionary<string, string> Artists { get; set; } = new();

        [JsonPropertyName("albums")]
        public Dictionary<string, string> Albums { get; set; } = new();

        [JsonPropertyName("songs")]
        public List<Song> Songs { get; set; } = new();
    }

    public class Song
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("artist")]
        public string Artist { get; set; } = string.Empty;

        [JsonPropertyName("album")]
        public string Album { get; set; } = string.Empty;

        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("genre")]
        public string Genre { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("wordCount")]
        public int WordCount { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}
