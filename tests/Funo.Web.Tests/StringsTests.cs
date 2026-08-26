using Funo.Core.Model;
using Funo.Web.Localization;

namespace Funo.Web.Tests;

public class StringsTests
{
    [Fact]
    public void Get_TranslatesToRequestedLanguage()
    {
        Assert.Equal("Kart cek", Strings.Get("table.drawCard", Lang.Tr));
        Assert.Equal("Draw card", Strings.Get("table.drawCard", Lang.En));
    }

    [Fact]
    public void Get_UnknownKey_ReturnsKeyItself()
    {
        // Bilinmeyen anahtarlar sessizce kaybolmamali; hata ayiklamada goze carpsin.
        Assert.Equal("some.unknown.key", Strings.Get("some.unknown.key", Lang.Tr));
    }

    [Fact]
    public void Get_WithArgs_FormatsPlaceholders()
    {
        Assert.Equal("3 kart", Strings.Get("table.cards", Lang.Tr, 3));
        Assert.Equal("3 cards", Strings.Get("table.cards", Lang.En, 3));
    }

    [Fact]
    public void Card_NumberCard_TranslatesColorAndValue()
    {
        var card = Card.Number(CardColor.Red, 7);

        Assert.Equal("Kirmizi 7", Strings.Card(card, Lang.Tr));
        Assert.Equal("Red 7", Strings.Card(card, Lang.En));
    }

    [Fact]
    public void Card_WildDrawFour_TranslatesWithoutColor()
    {
        var card = Card.WildDrawFour();

        Assert.Equal("Joker +4", Strings.Card(card, Lang.Tr));
        Assert.Equal("Wild +4", Strings.Card(card, Lang.En));
    }

    [Fact]
    public void Log_PlainArgs_AreNotTranslated()
    {
        // Oyuncu adlari gibi duz metin argumanlar oldugu gibi kalmali.
        var entry = new LogEntry("log.joined", "Bahriye");

        Assert.Equal("Bahriye odaya katildi.", Strings.Log(entry, Lang.Tr));
        Assert.Equal("Bahriye joined the room.", Strings.Log(entry, Lang.En));
    }

    [Fact]
    public void Log_CardToken_IsTranslatedPerLanguage()
    {
        var card = Card.Action(CardColor.Blue, CardType.Skip);
        var entry = new LogEntry("log.played", "Ahmet", Strings.Token.Card(card));

        Assert.Equal("Ahmet: Mavi Pas", Strings.Log(entry, Lang.Tr));
        Assert.Equal("Ahmet: Blue Skip", Strings.Log(entry, Lang.En));
    }

    [Fact]
    public void Log_ColorToken_IsTranslatedPerLanguage()
    {
        var entry = new LogEntry("log.playedColor", "Ahmet", Strings.Token.Card(Card.Wild()), Strings.Token.Color(CardColor.Green));

        Assert.Equal("Ahmet: Joker (renk: Yesil)", Strings.Log(entry, Lang.Tr));
        Assert.Equal("Ahmet: Wild (color: Green)", Strings.Log(entry, Lang.En));
    }

    [Fact]
    public void LanguageState_DefaultsToTurkish()
    {
        var state = new LanguageState();

        Assert.Equal(Lang.Tr, state.Current);
        Assert.Equal("tr", state.Code);
    }

    [Fact]
    public void LanguageState_Set_RaisesChangedEvent()
    {
        var state = new LanguageState();
        int raised = 0;
        state.Changed += () => raised++;

        state.Set(Lang.En);

        Assert.Equal(Lang.En, state.Current);
        Assert.Equal("en", state.Code);
        Assert.Equal(1, raised);
    }

    [Fact]
    public void LanguageState_SettingSameLanguage_DoesNotRaiseChanged()
    {
        var state = new LanguageState();
        int raised = 0;
        state.Changed += () => raised++;

        state.Set(Lang.Tr); // zaten varsayilan

        Assert.Equal(0, raised);
    }

    [Fact]
    public void LanguageState_Toggle_SwitchesBetweenTrAndEn()
    {
        var state = new LanguageState();

        state.Toggle();
        Assert.Equal(Lang.En, state.Current);

        state.Toggle();
        Assert.Equal(Lang.Tr, state.Current);
    }
}
