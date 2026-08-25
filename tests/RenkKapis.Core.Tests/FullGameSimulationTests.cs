using RenkKapis.Core.Ai;
using RenkKapis.Core.Engine;
using RenkKapis.Core.Model;

namespace RenkKapis.Core.Tests;

/// <summary>
/// Butunlesme testleri: motorun bastan sona kilitlenmeden calistigini dogrular.
/// Cok sayida rastgele oyun oynatarak nadir kural durumlarini yakalar.
/// </summary>
public class FullGameSimulationTests
{
    private static (bool finished, int turns, GameState state) PlayFullGame(int seed, int playerCount)
    {
        var random = new Random(seed);
        var players = Enumerable.Range(1, playerCount)
            .Select(i => new Player($"Bot{i}", isBot: true));

        var state = GameEngine.StartNew(players, GameOptions.Default, random);
        var bot = new SimpleBot(random);

        int turns = 0;
        const int maxTurns = 3000;

        while (!state.IsFinished && turns < maxTurns)
        {
            turns++;
            var player = state.CurrentPlayer;
            var choice = bot.ChooseCard(state, player);

            if (choice is null)
            {
                var draw = GameEngine.DrawCard(state);
                if (draw.CanPlayDrawnCard)
                    Play(state, bot, player, draw.DrawnCards[0]);
                continue;
            }

            Play(state, bot, player, choice);
        }

        return (state.IsFinished, turns, state);
    }

    private static void Play(GameState state, SimpleBot bot, Player player, Card card)
    {
        if (player.CardCount == 2)
            GameEngine.CallUno(state, player);

        CardColor? color = card.IsWild ? bot.ChooseColor(player) : null;
        var result = GameEngine.PlayCard(state, card, color);

        Assert.True(result.Success, $"Gecerli hamle reddedildi: {card} -> {result.Message}");
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(6)]
    public void Farkli_oyuncu_sayilarinda_oyun_tamamlanir(int playerCount)
    {
        for (int seed = 0; seed < 100; seed++)
        {
            var (finished, turns, state) = PlayFullGame(seed, playerCount);

            Assert.True(finished, $"Oyun bitmedi. seed={seed}, oyuncu={playerCount}, hamle={turns}");
            Assert.NotNull(state.Winner);
            Assert.Equal(0, state.Winner!.CardCount);
        }
    }

    [Fact]
    public void Oyun_boyunca_toplam_kart_sayisi_korunur()
    {
        for (int seed = 0; seed < 100; seed++)
        {
            var (_, _, state) = PlayFullGame(seed, 4);

            int total = state.Players.Sum(p => p.CardCount)
                        + state.Deck.DrawPileCount
                        + state.Deck.DiscardPileCount;

            Assert.Equal(108, total);
        }
    }

    [Fact]
    public void Kazanan_dogru_puani_alir()
    {
        for (int seed = 0; seed < 50; seed++)
        {
            var (_, _, state) = PlayFullGame(seed, 4);

            int expected = state.Players.Where(p => p != state.Winner).Sum(p => p.HandPoints);
            Assert.Equal(expected, state.Winner!.Score);
        }
    }
}
