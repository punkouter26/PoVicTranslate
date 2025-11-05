using FluentAssertions;
using Po.VicTranslate.Client.Services;
using Xunit;

namespace Po.VicTranslate.UnitTests.Services;

public class FuzzyMatchServiceTests
{
    private readonly FuzzyMatchService _service;

    public FuzzyMatchServiceTests()
    {
        _service = new FuzzyMatchService();
    }

    #region FindMatches - Basic Functionality

    [Fact]
    public void FindMatches_WithNullQuery_ReturnsEmptyList()
    {
        // Arrange
        var candidates = new[] { "apple", "banana", "cherry" };

        // Act
        var result = _service.FindMatches(null!, candidates);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindMatches_WithEmptyQuery_ReturnsEmptyList()
    {
        // Arrange
        var candidates = new[] { "apple", "banana", "cherry" };

        // Act
        var result = _service.FindMatches("", candidates);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindMatches_WithWhitespaceQuery_ReturnsEmptyList()
    {
        // Arrange
        var candidates = new[] { "apple", "banana", "cherry" };

        // Act
        var result = _service.FindMatches("   ", candidates);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindMatches_WithEmptyCandidates_ReturnsEmptyList()
    {
        // Arrange
        var candidates = Array.Empty<string>();

        // Act
        var result = _service.FindMatches("test", candidates);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindMatches_SkipsNullAndWhitespaceCandidates()
    {
        // Arrange
        var candidates = new[] { "apple", null!, "  ", "banana", "" };

        // Act
        var result = _service.FindMatches("app", candidates);

        // Assert
        result.Should().HaveCount(1);
        result[0].Text.Should().Be("apple");
    }

    #endregion

    #region FindMatches - Exact and Prefix Matches

    [Fact]
    public void FindMatches_WithExactMatch_ReturnsHighScore()
    {
        // Arrange
        var candidates = new[] { "apple", "application", "apply" };

        // Act
        var result = _service.FindMatches("apple", candidates);

        // Assert
        result.Should().NotBeEmpty();
        var exactMatch = result.First(m => m.Text == "apple");
        exactMatch.Score.Should().BeGreaterThan(0.9);
    }

    [Fact]
    public void FindMatches_WithPrefixMatch_BoostsScore()
    {
        // Arrange
        var candidates = new[] { "application", "reapply", "apple" };

        // Act
        var result = _service.FindMatches("app", candidates);

        // Assert
        result.Should().NotBeEmpty();
        var prefixMatches = result.Where(m => m.Text.StartsWith("app", StringComparison.OrdinalIgnoreCase)).ToList();
        prefixMatches.Should().HaveCountGreaterThan(0);
        
        // Prefix matches should score higher than non-prefix matches
        var prefixMatch = result.First(m => m.Text == "application");
        var nonPrefixMatch = result.FirstOrDefault(m => m.Text == "reapply");
        
        if (nonPrefixMatch != null)
        {
            prefixMatch.Score.Should().BeGreaterThan(nonPrefixMatch.Score);
        }
    }

    [Fact]
    public void FindMatches_IsCaseInsensitive()
    {
        // Arrange
        var candidates = new[] { "APPLE", "Banana", "cHeRrY" };

        // Act
        var result = _service.FindMatches("apple", candidates);

        // Assert
        result.Should().HaveCount(1);
        result[0].Text.Should().Be("APPLE");
    }

    [Fact]
    public void FindMatches_TrimsQuery()
    {
        // Arrange
        var candidates = new[] { "apple", "banana" };

        // Act
        var result = _service.FindMatches("  apple  ", candidates);

        // Assert
        result.Should().NotBeEmpty();
        result[0].Text.Should().Be("apple");
    }

    #endregion

    #region FindMatches - Word Boundary Matches

    [Fact]
    public void FindMatches_WithWordBoundaryMatch_BoostsScore()
    {
        // Arrange
        var candidates = new[] { "Wu-Tang Clan - C.R.E.A.M.", "Ice Cream", "Screaming" };

        // Act
        var result = _service.FindMatches("cream", candidates, threshold: 0.3);

        // Assert
        result.Should().NotBeEmpty();
        
        // "Ice Cream" should rank highest due to word boundary match
        var iceCreamMatch = result.FirstOrDefault(m => m.Text == "Ice Cream");
        iceCreamMatch.Should().NotBeNull();
        
        // If Screaming matches, Ice Cream should score higher due to word boundary
        var screamingMatch = result.FirstOrDefault(m => m.Text == "Screaming");
        if (screamingMatch != null)
        {
            iceCreamMatch!.Score.Should().BeGreaterThan(screamingMatch.Score);
        }
    }

    #endregion

    #region FindMatches - Levenshtein Distance

    [Fact]
    public void FindMatches_WithTypo_FindsSimilarStrings()
    {
        // Arrange
        var candidates = new[] { "protect ya neck", "method man", "shimmy shimmy ya" };

        // Act - "protekt" is a typo for "protect"
        var result = _service.FindMatches("protekt", candidates);

        // Assert
        result.Should().NotBeEmpty();
        result[0].Text.Should().Be("protect ya neck");
    }

    [Fact]
    public void FindMatches_WithPartialMatch_FindsRelevantResults()
    {
        // Arrange
        var candidates = new[] { 
            "Wu-Tang Clan Ain't Nuthing Ta F' Wit",
            "C.R.E.A.M.",
            "Protect Ya Neck"
        };

        // Act
        var result = _service.FindMatches("tang", candidates, threshold: 0.3);

        // Assert
        result.Should().NotBeEmpty();
        result[0].Text.Should().Be("Wu-Tang Clan Ain't Nuthing Ta F' Wit");
    }

    [Fact]
    public void FindMatches_WithLowSimilarity_FiltersOutByThreshold()
    {
        // Arrange
        var candidates = new[] { "xyz", "abc", "def" };

        // Act - "apple" has very low similarity to these candidates
        var result = _service.FindMatches("apple", candidates, threshold: 0.4);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region FindMatches - Sorting and Limiting

    [Fact]
    public void FindMatches_SortsByScoreDescending()
    {
        // Arrange
        var candidates = new[] { "app", "apple", "application", "apply" };

        // Act
        var result = _service.FindMatches("app", candidates);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().BeInDescendingOrder(m => m.Score);
    }

    [Fact]
    public void FindMatches_WithEqualScores_PrefersShortestMatch()
    {
        // Arrange
        var candidates = new[] { "testing application", "test" };

        // Act
        var result = _service.FindMatches("test", candidates);

        // Assert
        result.Should().NotBeEmpty();
        // When scores are very similar, shorter match should come first
        var firstMatch = result.First();
        firstMatch.Text.Should().Be("test");
    }

    [Fact]
    public void FindMatches_RespectsMaxResults()
    {
        // Arrange
        var candidates = new[] { "apple", "apply", "application", "applet", "appliance" };

        // Act
        var result = _service.FindMatches("app", candidates, maxResults: 3);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void FindMatches_WithFewerCandidatesThanMaxResults_ReturnsAllMatches()
    {
        // Arrange
        var candidates = new[] { "apple", "apply" };

        // Act
        var result = _service.FindMatches("app", candidates, maxResults: 10);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void FindMatches_RespectsThreshold()
    {
        // Arrange
        var candidates = new[] { "apple", "banana", "cherry" };

        // Act - High threshold should filter out poor matches
        var resultHighThreshold = _service.FindMatches("app", candidates, threshold: 0.95);
        var resultLowThreshold = _service.FindMatches("app", candidates, threshold: 0.3);

        // Assert
        resultHighThreshold.Should().HaveCountLessThanOrEqualTo(resultLowThreshold.Count);
    }

    #endregion

    #region FindMatches - Real-World Scenarios

    [Fact]
    public void FindMatches_WithWuTangSongTitles_FindsCorrectMatches()
    {
        // Arrange
        var candidates = new[]
        {
            "Wu-Tang Clan Ain't Nuthing Ta F' Wit",
            "C.R.E.A.M.",
            "Protect Ya Neck",
            "Method Man",
            "Shame on a Nigga",
            "Da Mystery Of Chessboxin'",
            "Wu-Tang: 7th Chamber",
            "Can It Be All So Simple",
            "Tearz"
        };

        // Act
        var result = _service.FindMatches("method", candidates);

        // Assert
        result.Should().NotBeEmpty();
        result[0].Text.Should().Be("Method Man");
    }

    [Fact]
    public void FindMatches_WithMisspelledSongTitle_FindsCorrectMatch()
    {
        // Arrange
        var candidates = new[]
        {
            "Protect Ya Neck",
            "Method Man",
            "C.R.E.A.M."
        };

        // Act - "protec" is missing the 't'
        var result = _service.FindMatches("protec", candidates);

        // Assert
        result.Should().NotBeEmpty();
        result[0].Text.Should().Be("Protect Ya Neck");
    }

    [Fact]
    public void FindMatches_WithAcronym_FindsFullTitle()
    {
        // Arrange
        var candidates = new[]
        {
            "C.R.E.A.M.",
            "Wu-Tang Clan",
            "Method Man"
        };

        // Act
        var result = _service.FindMatches("cream", candidates);

        // Assert
        result.Should().NotBeEmpty();
        result[0].Text.Should().Be("C.R.E.A.M.");
    }

    #endregion

    #region FuzzyMatch Model Tests

    [Fact]
    public void FuzzyMatch_ScorePercent_FormatsCorrectly()
    {
        // Arrange
        var match = new FuzzyMatch
        {
            Text = "test",
            Score = 0.853,
            MatchIndices = new List<int>()
        };

        // Act
        var percent = match.ScorePercent;

        // Assert
        percent.Should().Be("85%");
    }

    [Fact]
    public void FuzzyMatch_ScorePercent_RoundsCorrectly()
    {
        // Arrange
        var match = new FuzzyMatch
        {
            Text = "test",
            Score = 0.996,
            MatchIndices = new List<int>()
        };

        // Act
        var percent = match.ScorePercent;

        // Assert
        percent.Should().Be("100%");
    }

    [Fact]
    public void FuzzyMatch_DefaultValues_AreInitialized()
    {
        // Act
        var match = new FuzzyMatch();

        // Assert
        match.Text.Should().Be(string.Empty);
        match.Score.Should().Be(0.0);
        match.MatchIndices.Should().NotBeNull();
        match.MatchIndices.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void FindMatches_WithSingleCharacterQuery_FindsMatches()
    {
        // Arrange
        var candidates = new[] { "apple", "banana", "apricot" };

        // Act
        var result = _service.FindMatches("a", candidates);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void FindMatches_WithVeryLongQuery_HandlesGracefully()
    {
        // Arrange
        var candidates = new[] { "short", "medium length", "a very long candidate string" };
        var longQuery = new string('x', 1000);

        // Act
        var result = _service.FindMatches(longQuery, candidates);

        // Assert
        result.Should().BeEmpty(); // Should not crash
    }

    [Fact]
    public void FindMatches_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var candidates = new[] { "Wu-Tang Clan", "Method Man", "C.R.E.A.M." };

        // Act
        var result = _service.FindMatches("c.r.e.a.m", candidates);

        // Assert
        result.Should().NotBeEmpty();
        result[0].Text.Should().Be("C.R.E.A.M.");
    }

    [Fact]
    public void FindMatches_WithUnicodeCharacters_HandlesCorrectly()
    {
        // Arrange
        var candidates = new[] { "café", "naïve", "résumé" };

        // Act
        var result = _service.FindMatches("cafe", candidates);

        // Assert
        result.Should().NotBeEmpty();
        // Note: May or may not match "café" depending on normalization
    }

    [Fact]
    public void FindMatches_WithNumbers_HandlesCorrectly()
    {
        // Arrange
        var candidates = new[] { "Wu-Tang: 7th Chamber", "36 Chambers", "Method Man" };

        // Act
        var result = _service.FindMatches("7th", candidates, threshold: 0.3);

        // Assert
        result.Should().NotBeEmpty();
        result[0].Text.Should().Be("Wu-Tang: 7th Chamber");
    }

    #endregion
}
