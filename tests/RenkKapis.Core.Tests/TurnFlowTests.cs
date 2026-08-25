using RenkKapis.Core.Engine;
using RenkKapis.Core.Model;

namespace RenkKapis.Core.Tests;

public class TurnFlowTests
{
    [Fact]
    public void Normal_kart_sirayi_bir_ilerletir()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B", "C"]);
        TestHelper.SetHand(state.Players[0], Card.Number(CardColor.Red, 7), Card.Number(CardColor.Blue, 1));

        var result = GameEngine.PlayCard(state, Card.Number(CardColor.Red, 7));

        Assert.True(result.Success);
        Assert.Equal(1, state.CurrentPlayerIndex);   // A -> B
    }

    [Fact]
    public void Pas_karti_bir_oyuncuyu_atlar()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B", "C"]);
        TestHelper.SetHand(state.Players[0], Card.Action(CardColor.Red, CardType.Skip), Card.Number(CardColor.Blue, 1));

        GameEngine.PlayCard(state, Card.Action(CardColor.Red, CardType.Skip));

        Assert.Equal(2, state.CurrentPlayerIndex);   // B atlandi, sira C'de
    }

    [Fact]
    public void Ters_karti_yonu_degistirir()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B", "C"]);
        TestHelper.SetHand(state.Players[0], Card.Action(CardColor.Red, CardType.Reverse), Card.Number(CardColor.Blue, 1));

        GameEngine.PlayCard(state, Card.Action(CardColor.Red, CardType.Reverse));

        Assert.Equal(-1, state.Direction);
        Assert.Equal(2, state.CurrentPlayerIndex);   // ters yonde A -> C
    }

    [Fact]
    public void Iki_kisilik_oyunda_ters_pas_gibi_davranir()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B"]);
        TestHelper.SetHand(state.Players[0], Card.Action(CardColor.Red, CardType.Reverse), Card.Number(CardColor.Blue, 1));

        GameEngine.PlayCard(state, Card.Action(CardColor.Red, CardType.Reverse));

        Assert.Equal(0, state.CurrentPlayerIndex);   // sira yine A'da
    }

    [Fact]
    public void Iki_karti_cezayi_biriktirir_ve_sirayi_gecirir()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B", "C"]);
        TestHelper.SetHand(state.Players[0], Card.Action(CardColor.Red, CardType.DrawTwo), Card.Number(CardColor.Blue, 1));

        GameEngine.PlayCard(state, Card.Action(CardColor.Red, CardType.DrawTwo));

        Assert.Equal(2, state.PendingDrawCount);
        Assert.Equal(CardType.DrawTwo, state.PendingDrawType);
        Assert.Equal(1, state.CurrentPlayerIndex);
    }

    [Fact]
    public void Iki_ceza_uzerine_iki_ceza_birikir()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B", "C"]);
        TestHelper.SetHand(state.Players[0], Card.Action(CardColor.Red, CardType.DrawTwo), Card.Number(CardColor.Blue, 1));
        TestHelper.SetHand(state.Players[1], Card.Action(CardColor.Green, CardType.DrawTwo), Card.Number(CardColor.Blue, 2));

        GameEngine.PlayCard(state, Card.Action(CardColor.Red, CardType.DrawTwo));
        GameEngine.PlayCard(state, Card.Action(CardColor.Green, CardType.DrawTwo));

        Assert.Equal(4, state.PendingDrawCount);
        Assert.Equal(2, state.CurrentPlayerIndex);   // sira C'de, 4 kart cekmeli
    }

    [Fact]
    public void Ceza_cekilince_sira_gecer_ve_ceza_sifirlanir()
    {
        var state = TestHelper.CreateState(Card.Action(CardColor.Red, CardType.DrawTwo),
            playerNames: ["A", "B", "C"]);
        state.PendingDrawCount = 4;
        state.PendingDrawType = CardType.DrawTwo;
        TestHelper.SetHand(state.Players[0], Card.Number(CardColor.Blue, 1));

        var result = GameEngine.DrawCard(state);

        Assert.Equal(4, result.DrawnCards.Count);
        Assert.Equal(5, state.Players[0].CardCount);
        Assert.Equal(0, state.PendingDrawCount);
        Assert.Null(state.PendingDrawType);
        Assert.Equal(1, state.CurrentPlayerIndex);
    }

    [Fact]
    public void Joker_oynanirken_secilen_renk_aktif_olur()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B"]);
        TestHelper.SetHand(state.Players[0], Card.Wild(), Card.Number(CardColor.Blue, 1));

        var result = GameEngine.PlayCard(state, Card.Wild(), CardColor.Green);

        Assert.True(result.Success);
        Assert.Equal(CardColor.Green, state.ActiveColor);
    }

    [Fact]
    public void Joker_renk_secilmeden_oynanamaz()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B"]);
        TestHelper.SetHand(state.Players[0], Card.Wild(), Card.Number(CardColor.Blue, 1));

        var result = GameEngine.PlayCard(state, Card.Wild());

        Assert.False(result.Success);
        Assert.Equal(2, state.Players[0].CardCount);   // kart elden cikmadi
    }

    [Fact]
    public void Elde_olmayan_kart_oynanamaz()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B"]);
        TestHelper.SetHand(state.Players[0], Card.Number(CardColor.Red, 7));

        var result = GameEngine.PlayCard(state, Card.Number(CardColor.Red, 9));

        Assert.False(result.Success);
        Assert.Equal(0, state.CurrentPlayerIndex);   // sira degismedi
    }

    [Fact]
    public void Oynanamayan_kart_cekilince_sira_gecer()
    {
        var state = TestHelper.CreateState(Card.Number(CardColor.Red, 5),
            playerNames: ["A", "B"]);
        TestHelper.SetHand(state.Players[0], Card.Number(CardColor.Blue, 1));

        int before = state.Players[0].CardCount;
        GameEngine.DrawCard(state);

        Assert.Equal(before + 1, state.Players[0].CardCount);
    }
}
