using RenkKapis.Core.Model;

namespace RenkKapis.Core.Tests;

public class DeckTests
{
    [Fact]
    public void Yeni_deste_108_kart_icerir()
    {
        var deck = new Deck(new Random(42));
        Assert.Equal(108, deck.DrawPileCount);
    }

    [Fact]
    public void Deste_dogru_kart_dagilimina_sahiptir()
    {
        var deck = new Deck(new Random(42));
        var all = new List<Card>();
        for (int i = 0; i < 108; i++)
            all.Add(deck.Draw());

        Assert.Equal(76, all.Count(c => c.Type == CardType.Number));
        Assert.Equal(4, all.Count(c => c.Type == CardType.Number && c.Value == 0));
        Assert.Equal(8, all.Count(c => c.Type == CardType.Skip));
        Assert.Equal(8, all.Count(c => c.Type == CardType.Reverse));
        Assert.Equal(8, all.Count(c => c.Type == CardType.DrawTwo));
        Assert.Equal(4, all.Count(c => c.Type == CardType.Wild));
        Assert.Equal(4, all.Count(c => c.Type == CardType.WildDrawFour));
    }

    [Fact]
    public void Ayni_tohum_ayni_karistirma_uretir()
    {
        var a = new Deck(new Random(7));
        var b = new Deck(new Random(7));

        for (int i = 0; i < 20; i++)
            Assert.Equal(a.Draw(), b.Draw());
    }

    [Fact]
    public void Deste_bitince_atilan_yigin_yeniden_karistirilir()
    {
        var deck = new Deck(new Random(1));

        // Tum desteyi cekip atilan yigina koy
        for (int i = 0; i < 108; i++)
            deck.Discard(deck.Draw());

        Assert.Equal(0, deck.DrawPileCount);
        Assert.Equal(108, deck.DiscardPileCount);

        var top = deck.TopDiscard;
        var card = deck.Draw();

        Assert.NotNull(card);
        Assert.Equal(106, deck.DrawPileCount);      // 107 geri dondu, 1 tanesi cekildi
        Assert.Equal(top, deck.TopDiscard);          // en ust kart korundu
    }

    [Fact]
    public void Cekilecek_kart_kalmayinca_hata_firlatir()
    {
        var deck = new Deck(new Random(1));
        for (int i = 0; i < 108; i++)
            deck.Draw();   // hicbiri atilan yigina gitmiyor

        Assert.Throws<InvalidOperationException>(() => deck.Draw());
    }
}
