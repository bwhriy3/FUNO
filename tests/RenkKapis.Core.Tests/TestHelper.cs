using RenkKapis.Core.Model;

namespace RenkKapis.Core.Tests;

/// <summary>
/// Testlerde kontrollu oyun durumu kurmak icin yardimci.
/// Gercek dagitim yerine el ve ust kart dogrudan belirlenir.
/// </summary>
internal static class TestHelper
{
    public static GameState CreateState(
        Card topCard,
        CardColor? activeColor = null,
        GameOptions? options = null,
        params string[] playerNames)
    {
        if (playerNames.Length == 0)
            playerNames = ["Ali", "Veli"];

        var players = playerNames.Select(n => new Player(n)).ToList();
        var deck = new Deck(new Random(123));
        var state = new GameState(players, deck, options ?? GameOptions.Default);

        deck.Discard(topCard);
        state.ActiveColor = activeColor ?? topCard.Color;

        return state;
    }

    /// <summary>Oyuncunun elini sifirdan belirler.</summary>
    public static void SetHand(Player player, params Card[] cards)
    {
        foreach (var c in player.Hand.ToList())
            player.RemoveCard(c);
        player.AddCards(cards);
    }
}
