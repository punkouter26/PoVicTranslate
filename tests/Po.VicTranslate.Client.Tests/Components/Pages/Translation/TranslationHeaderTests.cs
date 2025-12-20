using Bunit;
using FluentAssertions;
using Xunit;

namespace Po.VicTranslate.Client.Tests.Components.Pages.Translation;

public class TranslationHeaderTests : TestContext
{
    [Fact]
    public void TranslationHeader_RendersCorrectly()
    {
        // Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.TranslationHeader>();

        // Assert
        cut.Find(".hero-section").Should().NotBeNull();
        cut.Find(".urban-title").Should().NotBeNull();
        cut.Find(".title-main").TextContent.Should().Be("Street → Sophisticated");
        cut.Find(".title-sub").TextContent.Should().Be("Transform rap lyrics into Victorian elegance");
    }

    [Fact]
    public void TranslationHeader_DisplaysCorrectStats()
    {
        // Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.TranslationHeader>();

        // Assert
        var stats = cut.FindAll(".stat-item");
        stats.Should().HaveCount(3);
        
        stats[0].TextContent.Should().Contain("246 Songs");
        stats[1].TextContent.Should().Contain("AI Powered");
        stats[2].TextContent.Should().Contain("Wu-Tang Edition");
    }

    [Fact]
    public void TranslationHeader_HasProperIcons()
    {
        // Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.TranslationHeader>();

        // Assert - Now using emoji icons instead of Font Awesome
        var statIcons = cut.FindAll(".stat-icon");
        statIcons.Should().HaveCount(3);
        statIcons[0].TextContent.Should().Contain("🎵");
        statIcons[1].TextContent.Should().Contain("✨");
        statIcons[2].TextContent.Should().Contain("👑");
    }
}
