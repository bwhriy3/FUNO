using Funo.Core.Engine;
using Funo.Core.Model;

namespace Funo.Core.Tests;

public class CanPlayTests
{
    [Fact]
    public void Ayni_renk_oynanabilir()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5));
        Assert.True(GameEngine.CanPlay(state, Card.Number(CardColor.Red, 9)));
    }

    [Fact]
    public void Ayni_sayi_farkli_renk_oynanabilir()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5));
        Assert.True(GameEngine.CanPlay(state, Card.Number(CardColor.Blue, 5)));
    }

    [Fact]
    public void Farkli_renk_farkli_sayi_oynanamaz()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5));
        Assert.False(GameEngine.CanPlay(state, Card.Number(CardColor.Blue, 8)));
    }

    [Fact]
    public void Joker_her_zaman_oynanabilir()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5));
        Assert.True(GameEngine.CanPlay(state, Card.Wild()));
        Assert.True(GameEngine.CanPlay(state, Card.WildDrawFour()));
    }

    [Fact]
    public void Ayni_aksiyon_tipi_farkli_renk_oynanabilir()
    {
        var state = TestHelper.CreateState(Card.Action(CardColor.Red, CardType.Skip));
        Assert.True(GameEngine.CanPlay(state, Card.Action(CardColor.Green, CardType.Skip)));
    }

    [Fact]
    public void Joker_sonrasi_secilen_renk_gecerlidir()
    {
        // Ust kart joker ama aktif renk mavi secilmis
        var state = TestHelper.CreateState(Card.Wild(), CardColor.Blue);

        Assert.True(GameEngine.CanPlay(state, Card.Number(CardColor.Blue, 3)));
        Assert.False(GameEngine.CanPlay(state, Card.Number(CardColor.Red, 3)));
    }

    [Fact]
    public void Ceza_birikmisken_sadece_ayni_tip_ceza_karti_oynanabilir()
    {
        var state = TestHelper.CreateState(Card.Action(CardColor.Red, CardType.DrawTwo));
        state.PendingDrawCount = 2;
        state.PendingDrawType = CardType.DrawTwo;

        Assert.True(GameEngine.CanPlay(state, Card.Action(CardColor.Blue, CardType.DrawTwo)));
        Assert.False(GameEngine.CanPlay(state, Card.Number(CardColor.Red, 5)));
        Assert.False(GameEngine.CanPlay(state, Card.Wild()));
    }

    [Fact]
    public void Stack_kapaliyken_ceza_uzerine_kart_oynanamaz()
    {
        var options = GameOptions.Default with { StackDrawTwo = false };
        var state = TestHelper.CreateState(Card.Action(CardColor.Red, CardType.DrawTwo), options: options);
        state.PendingDrawCount = 2;
        state.PendingDrawType = CardType.DrawTwo;

        Assert.False(GameEngine.CanPlay(state, Card.Action(CardColor.Blue, CardType.DrawTwo)));
    }

    [Fact]
    public void Dort_ceza_uzerine_dort_varsayilan_olarak_oynanamaz()
    {
        var state = TestHelper.CreateState(Card.WildDrawFour(), CardColor.Red);
        state.PendingDrawCount = 4;
        state.PendingDrawType = CardType.WildDrawFour;

        Assert.False(GameEngine.CanPlay(state, Card.WildDrawFour()));
    }
}
