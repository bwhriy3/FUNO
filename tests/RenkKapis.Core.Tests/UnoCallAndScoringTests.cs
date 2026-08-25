using RenkKapis.Core.Engine;
using RenkKapis.Core.Model;

namespace RenkKapis.Core.Tests;

public class UnoCallAndScoringTests
{
    [Fact]
    public void Tek_demeden_son_karta_dusen_oyuncu_ceza_ceker()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B"]);
        TestHelper.SetHand(state.Players[0], Card.Number(CardColor.Red, 7), Card.Number(CardColor.Blue, 1));

        GameEngine.PlayCard(state, Card.Number(CardColor.Red, 7));

        // 1 kart kalacakti, ceza olarak 2 kart daha cekti
        Assert.Equal(3, state.Players[0].CardCount);
    }

    [Fact]
    public void Tek_diyen_oyuncu_ceza_cekmez()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B"]);
        var a = state.Players[0];
        TestHelper.SetHand(a, Card.Number(CardColor.Red, 7), Card.Number(CardColor.Blue, 1));

        Assert.True(GameEngine.CallUno(state, a));
        GameEngine.PlayCard(state, Card.Number(CardColor.Red, 7));

        Assert.Equal(1, a.CardCount);
    }

    [Fact]
    public void Tek_demek_sadece_iki_kart_varken_gecerlidir()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B"]);
        var a = state.Players[0];
        TestHelper.SetHand(a, Card.Number(CardColor.Red, 7), Card.Number(CardColor.Blue, 1), Card.Number(CardColor.Blue, 2));

        Assert.False(GameEngine.CallUno(state, a));
        Assert.False(a.HasCalledUno);
    }

    [Fact]
    public void Kural_kapaliyken_tek_demeden_ceza_yoktur()
    {
        var options = GameOptions.Default with { EnforceUnoCall = false };
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5), options: options,
            playerNames: ["A", "B"]);
        TestHelper.SetHand(state.Players[0], Card.Number(CardColor.Red, 7), Card.Number(CardColor.Blue, 1));

        GameEngine.PlayCard(state, Card.Number(CardColor.Red, 7));

        Assert.Equal(1, state.Players[0].CardCount);
    }

    [Fact]
    public void Son_kart_oynaninca_oyun_biter()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B"]);
        var a = state.Players[0];
        TestHelper.SetHand(a, Card.Number(CardColor.Red, 7));
        TestHelper.SetHand(state.Players[1], Card.Number(CardColor.Blue, 3));

        var result = GameEngine.PlayCard(state, Card.Number(CardColor.Red, 7));

        Assert.True(result.Success);
        Assert.True(state.IsFinished);
        Assert.Equal(a, state.Winner);
    }

    [Fact]
    public void Kazanan_digerlerinin_el_puanini_alir()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B", "C"]);
        var a = state.Players[0];
        TestHelper.SetHand(a, Card.Number(CardColor.Red, 7));
        TestHelper.SetHand(state.Players[1], Card.Number(CardColor.Blue, 3));                    // 3 puan
        TestHelper.SetHand(state.Players[2], Card.WildDrawFour(), Card.Action(CardColor.Green, CardType.Skip)); // 50 + 20

        GameEngine.PlayCard(state, Card.Number(CardColor.Red, 7));

        Assert.Equal(73, a.Score);
    }

    [Fact]
    public void Oyun_bitince_baska_kart_oynanamaz()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B"]);
        TestHelper.SetHand(state.Players[0], Card.Number(CardColor.Red, 7));
        TestHelper.SetHand(state.Players[1], Card.Number(CardColor.Red, 3));

        GameEngine.PlayCard(state, Card.Number(CardColor.Red, 7));
        var result = GameEngine.PlayCard(state, Card.Number(CardColor.Red, 3));

        Assert.False(result.Success);
    }

    [Theory]
    [InlineData(CardType.Skip, 20)]
    [InlineData(CardType.Reverse, 20)]
    [InlineData(CardType.DrawTwo, 20)]
    public void Aksiyon_kartlari_yirmi_puandir(CardType type, int expected)
    {
        Assert.Equal(expected, Card.Action(CardColor.Red, type).Points);
    }

    [Fact]
    public void Joker_kartlari_elli_puandir()
    {
        Assert.Equal(50, Card.Wild().Points);
        Assert.Equal(50, Card.WildDrawFour().Points);
    }
}
