using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Po.VicTranslate.Client.Tests.Components.Pages.Translation;

public class CustomLyricsInputTests : TestContext
{
    [Fact]
    public void CustomLyricsInput_RendersTextarea()
    {
        // Arrange & Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.CustomLyricsInput>(parameters => parameters
            .Add(p => p.CurrentText, "Test lyrics")
            .Add(p => p.CurrentTextChanged, EventCallback.Factory.Create<string>(this, _ => { }))
            .Add(p => p.WordCount, 2)
            .Add(p => p.IsDisabled, false));

        // Assert
        var textarea = cut.Find("textarea");
        textarea.Should().NotBeNull();
        textarea.GetAttribute("placeholder").Should().Contain("Spit your verses here");
    }

    [Fact]
    public void CustomLyricsInput_WhenDisabled_DisablesTextarea()
    {
        // Arrange & Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.CustomLyricsInput>(parameters => parameters
            .Add(p => p.CurrentText, "")
            .Add(p => p.CurrentTextChanged, EventCallback.Factory.Create<string>(this, _ => { }))
            .Add(p => p.WordCount, 0)
            .Add(p => p.IsDisabled, true));

        // Assert
        var textarea = cut.Find("textarea");
        textarea.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void CustomLyricsInput_DisplaysProgressRing()
    {
        // Arrange & Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.CustomLyricsInput>(parameters => parameters
            .Add(p => p.CurrentText, "Word word word")
            .Add(p => p.CurrentTextChanged, EventCallback.Factory.Create<string>(this, _ => { }))
            .Add(p => p.WordCount, 3)
            .Add(p => p.IsDisabled, false));

        // Assert
        cut.FindComponent<Po.VicTranslate.Client.Components.Shared.ProgressRing>().Should().NotBeNull();
    }

    [Fact]
    public void CustomLyricsInput_PassesCorrectPropsToProgressRing()
    {
        // Arrange & Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.CustomLyricsInput>(parameters => parameters
            .Add(p => p.CurrentText, "")
            .Add(p => p.CurrentTextChanged, EventCallback.Factory.Create<string>(this, _ => { }))
            .Add(p => p.WordCount, 50)
            .Add(p => p.IsDisabled, false));

        // Assert
        var progressRing = cut.FindComponent<Po.VicTranslate.Client.Components.Shared.ProgressRing>();
        progressRing.Instance.CurrentValue.Should().Be(50);
        progressRing.Instance.MaxValue.Should().Be(200);
        progressRing.Instance.Size.Should().Be(80);
    }
}
