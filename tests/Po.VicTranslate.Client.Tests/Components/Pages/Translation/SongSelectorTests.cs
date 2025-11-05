using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Po.VicTranslate.Client.Tests.Components.Pages.Translation;

public class SongSelectorTests : TestContext
{
    [Fact]
    public void SongSelector_WithEmptyList_DisablesRandomButton()
    {
        // Arrange
        var availableSongs = new List<string>();

        // Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.SongSelector>(parameters => parameters
            .Add(p => p.AvailableSongs, availableSongs)
            .Add(p => p.OnSongSelected, EventCallback.Factory.Create<string>(this, _ => { }))
            .Add(p => p.OnRandomClick, EventCallback.Factory.Create(this, () => { })));

        // Assert
        var randomButton = cut.Find(".shuffle-btn");
        randomButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void SongSelector_WithSongs_EnablesRandomButton()
    {
        // Arrange
        var availableSongs = new List<string> { "song1.txt", "song2.txt" };

        // Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.SongSelector>(parameters => parameters
            .Add(p => p.AvailableSongs, availableSongs)
            .Add(p => p.OnSongSelected, EventCallback.Factory.Create<string>(this, _ => { }))
            .Add(p => p.OnRandomClick, EventCallback.Factory.Create(this, () => { })));

        // Assert
        var randomButton = cut.Find(".shuffle-btn");
        randomButton.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void SongSelector_RendersSongOptions()
    {
        // Arrange
        var availableSongs = new List<string> { "cream.txt", "protect_ya_neck.txt" };

        // Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.SongSelector>(parameters => parameters
            .Add(p => p.AvailableSongs, availableSongs)
            .Add(p => p.OnSongSelected, EventCallback.Factory.Create<string>(this, _ => { }))
            .Add(p => p.OnRandomClick, EventCallback.Factory.Create(this, () => { })));

        // Assert
        var options = cut.FindAll("option");
        options.Should().HaveCountGreaterThan(2); // Includes placeholder option
        options.Should().Contain(o => o.GetAttribute("value") == "cream.txt");
        options.Should().Contain(o => o.GetAttribute("value") == "protect_ya_neck.txt");
    }

    [Fact]
    public void SongSelector_OnSongChange_InvokesCallback()
    {
        // Arrange
        var availableSongs = new List<string> { "cream.txt" };
        var callbackInvoked = false;
        var selectedSong = string.Empty;

        // Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.SongSelector>(parameters => parameters
            .Add(p => p.AvailableSongs, availableSongs)
            .Add(p => p.OnSongSelected, EventCallback.Factory.Create<string>(this, song => {
                callbackInvoked = true;
                selectedSong = song;
            }))
            .Add(p => p.OnRandomClick, EventCallback.Factory.Create(this, () => { })));

        var select = cut.Find("select");
        select.Change("cream.txt");

        // Assert
        callbackInvoked.Should().BeTrue();
        selectedSong.Should().Be("cream.txt");
    }

    [Fact]
    public void SongSelector_OnRandomClick_InvokesCallback()
    {
        // Arrange
        var availableSongs = new List<string> { "cream.txt" };
        var callbackInvoked = false;

        // Act
        var cut = RenderComponent<Po.VicTranslate.Client.Components.Pages.Translation.SongSelector>(parameters => parameters
            .Add(p => p.AvailableSongs, availableSongs)
            .Add(p => p.OnSongSelected, EventCallback.Factory.Create<string>(this, _ => { }))
            .Add(p => p.OnRandomClick, EventCallback.Factory.Create(this, () => callbackInvoked = true)));

        var randomButton = cut.Find(".shuffle-btn");
        randomButton.Click();

        // Assert
        callbackInvoked.Should().BeTrue();
    }
}
