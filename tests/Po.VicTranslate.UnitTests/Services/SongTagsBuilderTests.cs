using FluentAssertions;
using Po.VicTranslate.Api.Services.Lyrics;
using Xunit;

namespace Po.VicTranslate.UnitTests.Services;

public class SongTagsBuilderTests
{
    [Fact]
    public void Build_WithNoConfiguration_ReturnsEmptyList()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder.Build();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void AddBaseTags_AddsHipHopAndWuTangTags()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder.AddBaseTags().Build();

        // Assert
        result.Should().Contain("hip-hop")
            .And.Contain("wu-tang")
            .And.HaveCount(2);
    }

    [Fact]
    public void WithFileName_StoresLowercaseFileName()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithFileName("CREAM_Remix")
            .AddFormatTags()
            .Build();

        // Assert
        result.Should().Contain("remix");
    }

    [Fact]
    public void WithContent_StoresLowercaseContent()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithContent("Shaolin SHADOWBOXING")
            .AddMartialArtsTags()
            .Build();

        // Assert
        result.Should().Contain("martial-arts");
    }

    [Fact]
    public void AddMartialArtsTags_WithShaolinInContent_AddsMartialArtsTag()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithContent("Shaolin shadowboxing and Wu-Tang sword style")
            .AddMartialArtsTags()
            .Build();

        // Assert
        result.Should().Contain("martial-arts");
    }

    [Fact]
    public void AddMartialArtsTags_WithKungFuInContent_AddsMartialArtsTag()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithContent("kung fu fighting")
            .AddMartialArtsTags()
            .Build();

        // Assert
        result.Should().Contain("martial-arts");
    }

    [Fact]
    public void AddMartialArtsTags_WithNoReferences_DoesNotAddTag()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithContent("cash rules everything around me")
            .AddMartialArtsTags()
            .Build();

        // Assert
        result.Should().NotContain("martial-arts");
    }

    [Fact]
    public void AddAlbumTags_WithChamberInContent_Adds36ChambersTag()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithContent("Enter the 36 chambers")
            .AddAlbumTags()
            .Build();

        // Assert
        result.Should().Contain("36-chambers");
    }

    [Fact]
    public void AddAlbumTags_WithNoChamberReference_DoesNotAddTag()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithContent("Wu-Tang is for the children")
            .AddAlbumTags()
            .Build();

        // Assert
        result.Should().NotContain("36-chambers");
    }

    [Fact]
    public void AddFormatTags_WithRemixInFileName_AddsRemixTag()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithFileName("protect_ya_neck_remix")
            .AddFormatTags()
            .Build();

        // Assert
        result.Should().Contain("remix");
    }

    [Fact]
    public void AddFormatTags_WithIntroInFileName_AddsInterludeTag()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithFileName("wu_tang_clan_intro")
            .AddFormatTags()
            .Build();

        // Assert
        result.Should().Contain("interlude");
    }

    [Fact]
    public void AddFormatTags_WithOutroInFileName_AddsInterludeTag()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithFileName("album_outro")
            .AddFormatTags()
            .Build();

        // Assert
        result.Should().Contain("interlude");
    }

    [Fact]
    public void AddFormatTags_WithNoFormatKeywords_DoesNotAddTags()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithFileName("cream")
            .AddFormatTags()
            .Build();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void AddClassicTags_WithCreamInContent_AddsClassicTag()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithContent("cream get the money")
            .AddClassicTags()
            .Build();

        // Assert
        result.Should().Contain("classic");
    }

    [Fact]
    public void AddClassicTags_WithCashRulesInContent_AddsClassicTag()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithContent("Cash Rules Everything Around Me")
            .AddClassicTags()
            .Build();

        // Assert
        result.Should().Contain("classic");
    }

    [Fact]
    public void AddClassicTags_WithNoClassicReferences_DoesNotAddTag()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithContent("Wu-Tang Clan ain't nuthing ta f' wit")
            .AddClassicTags()
            .Build();

        // Assert
        result.Should().NotContain("classic");
    }

    [Fact]
    public void Build_WithDuplicateTags_ReturnsDistinctList()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .AddBaseTags()
            .AddBaseTags()  // Add base tags twice
            .Build();

        // Assert
        result.Should().HaveCount(2)
            .And.OnlyHaveUniqueItems();
    }

    [Fact]
    public void FluentInterface_SupportsMethodChaining()
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithFileName("cream_remix")
            .WithContent("Cash rules everything around me in the 36 chambers with shaolin style")
            .AddBaseTags()
            .AddMartialArtsTags()
            .AddAlbumTags()
            .AddFormatTags()
            .AddClassicTags()
            .Build();

        // Assert
        result.Should().Contain("hip-hop")
            .And.Contain("wu-tang")
            .And.Contain("martial-arts")
            .And.Contain("36-chambers")
            .And.Contain("remix")
            .And.Contain("classic")
            .And.HaveCount(6);
    }

    [Fact]
    public void Build_WithTypicalSongData_GeneratesExpectedTags()
    {
        // Arrange
        var builder = new SongTagsBuilder();
        var fileName = "protect_ya_neck";
        var content = "Wu-Tang Clan comin' at ya from the 36 chambers";

        // Act
        var result = builder
            .WithFileName(fileName)
            .WithContent(content)
            .AddBaseTags()
            .AddMartialArtsTags()
            .AddAlbumTags()
            .AddFormatTags()
            .AddClassicTags()
            .Build();

        // Assert
        result.Should().Contain("hip-hop")
            .And.Contain("wu-tang")
            .And.Contain("36-chambers")
            .And.HaveCount(3);
    }

    [Theory]
    [InlineData("shaolin", "martial-arts")]
    [InlineData("kung fu", "martial-arts")]
    [InlineData("SHAOLIN", "martial-arts")]
    [InlineData("Kung Fu", "martial-arts")]
    public void AddMartialArtsTags_WithVariousCasing_HandlesCorrectly(string contentSnippet, string expectedTag)
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithContent(contentSnippet)
            .AddMartialArtsTags()
            .Build();

        // Assert
        result.Should().Contain(expectedTag);
    }

    [Theory]
    [InlineData("intro", "interlude")]
    [InlineData("outro", "interlude")]
    [InlineData("INTRO", "interlude")]
    [InlineData("OUTRO", "interlude")]
    [InlineData("remix", "remix")]
    [InlineData("REMIX", "remix")]
    public void AddFormatTags_WithVariousFormats_HandlesCorrectly(string fileNamePart, string expectedTag)
    {
        // Arrange
        var builder = new SongTagsBuilder();

        // Act
        var result = builder
            .WithFileName($"wu_tang_{fileNamePart}")
            .AddFormatTags()
            .Build();

        // Assert
        result.Should().Contain(expectedTag);
    }
}
