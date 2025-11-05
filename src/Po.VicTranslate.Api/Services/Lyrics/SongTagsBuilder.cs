namespace Po.VicTranslate.Api.Services;

/// <summary>
/// Builder for generating tags from song metadata and content.
/// Provides a fluent interface for composing tag generation logic.
/// </summary>
public class SongTagsBuilder
{
    private readonly List<string> _tags = [];
    private string _fileName = string.Empty;
    private string _content = string.Empty;

    /// <summary>
    /// Sets the filename to analyze for tag generation.
    /// </summary>
    public SongTagsBuilder WithFileName(string fileName)
    {
        _fileName = fileName.ToLowerInvariant();
        return this;
    }

    /// <summary>
    /// Sets the content to analyze for tag generation.
    /// </summary>
    public SongTagsBuilder WithContent(string content)
    {
        _content = content.ToLowerInvariant();
        return this;
    }

    /// <summary>
    /// Adds base genre tags (hip-hop, wu-tang).
    /// </summary>
    public SongTagsBuilder AddBaseTags()
    {
        _tags.Add("hip-hop");
        _tags.Add("wu-tang");
        return this;
    }

    /// <summary>
    /// Adds martial arts related tags if content contains references.
    /// </summary>
    public SongTagsBuilder AddMartialArtsTags()
    {
        if (_content.Contains("shaolin") || _content.Contains("kung fu"))
        {
            _tags.Add("martial-arts");
        }
        return this;
    }

    /// <summary>
    /// Adds album-specific tags (e.g., 36-chambers).
    /// </summary>
    public SongTagsBuilder AddAlbumTags()
    {
        if (_content.Contains("chamber"))
        {
            _tags.Add("36-chambers");
        }
        return this;
    }

    /// <summary>
    /// Adds format-specific tags (remix, interlude).
    /// </summary>
    public SongTagsBuilder AddFormatTags()
    {
        if (_fileName.Contains("remix"))
        {
            _tags.Add("remix");
        }

        if (_fileName.Contains("intro") || _fileName.Contains("outro"))
        {
            _tags.Add("interlude");
        }
        return this;
    }

    /// <summary>
    /// Adds classic song tags for well-known tracks.
    /// </summary>
    public SongTagsBuilder AddClassicTags()
    {
        if (_content.Contains("cream") || _content.Contains("cash rules"))
        {
            _tags.Add("classic");
        }
        return this;
    }

    /// <summary>
    /// Builds and returns the final list of distinct tags.
    /// </summary>
    public List<string> Build()
    {
        return _tags.Distinct().ToList();
    }
}
