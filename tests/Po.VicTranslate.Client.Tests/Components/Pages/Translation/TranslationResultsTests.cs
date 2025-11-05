using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Po.VicTranslate.Client.Tests.Components.Pages.Translation;

public class TranslationResultsTests : TestContext
{
    [Fact]
    public void TranslationResults_RendersTranslatedText()
    {
        // Arrange & Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.TranslationResults>(parameters => parameters
            .Add(p => p.TranslatedText, "Verily, this is Victorian text")
            .Add(p => p.IsEditMode, false)
            .Add(p => p.OnCopyClick, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnEnterEdit, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSaveEdit, EventCallback.Factory.Create<string>(this, _ => { }))
            .Add(p => p.OnCancelEdit, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnRetranslate, EventCallback.Factory.Create(this, () => { })));

        // Assert
        cut.Find(".results-section").Should().NotBeNull();
        cut.Find(".result-card").Should().NotBeNull();
    }

    [Fact]
    public void TranslationResults_HasCopyButton()
    {
        // Arrange & Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.TranslationResults>(parameters => parameters
            .Add(p => p.TranslatedText, "Victorian text")
            .Add(p => p.IsEditMode, false)
            .Add(p => p.OnCopyClick, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnEnterEdit, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSaveEdit, EventCallback.Factory.Create<string>(this, _ => { }))
            .Add(p => p.OnCancelEdit, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnRetranslate, EventCallback.Factory.Create(this, () => { })));

        // Assert
        var copyButton = cut.Find(".copy-btn");
        copyButton.Should().NotBeNull();
        copyButton.GetAttribute("title").Should().Be("Copy to clipboard");
    }

    [Fact]
    public void TranslationResults_OnCopyClick_InvokesCallback()
    {
        // Arrange
        var callbackInvoked = false;

        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.TranslationResults>(parameters => parameters
            .Add(p => p.TranslatedText, "Victorian text")
            .Add(p => p.IsEditMode, false)
            .Add(p => p.OnCopyClick, EventCallback.Factory.Create(this, () => callbackInvoked = true))
            .Add(p => p.OnEnterEdit, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSaveEdit, EventCallback.Factory.Create<string>(this, _ => { }))
            .Add(p => p.OnCancelEdit, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnRetranslate, EventCallback.Factory.Create(this, () => { })));

        // Act
        var copyButton = cut.Find(".copy-btn");
        copyButton.Click();

        // Assert
        callbackInvoked.Should().BeTrue();
    }

    [Fact]
    public void TranslationResults_ShowsAudioHint()
    {
        // Arrange & Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.TranslationResults>(parameters => parameters
            .Add(p => p.TranslatedText, "Victorian text")
            .Add(p => p.IsEditMode, false)
            .Add(p => p.OnCopyClick, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnEnterEdit, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSaveEdit, EventCallback.Factory.Create<string>(this, _ => { }))
            .Add(p => p.OnCancelEdit, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnRetranslate, EventCallback.Factory.Create(this, () => { })));

        // Assert
        var label = cut.Find(".results-label");
        label.TextContent.Should().Contain("Audio plays automatically");
    }
}
