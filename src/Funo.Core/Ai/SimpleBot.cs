using Funo.Core.Engine;
using Funo.Core.Model;

namespace Funo.Core.Ai;

/// <summary>
/// Basit bot stratejisi:
/// 1. Oynanabilir kartlar icinde once aksiyon kartlarini (Pas, Ters, +2) tercih eder.
/// 2. Sonra rengi tutan sayi kartlarindan en yuksek puanliyi oynar.
/// 3. Jokerleri en son kullanir.
/// Renk secerken elindeki en cok sahip oldugu rengi secer.
/// </summary>
public sealed class SimpleBot
{
    private readonly Random _random;

    public SimpleBot(Random? random = null) => _random = random ?? new Random();

    /// <summary>Botun oynayacagi karti secer. Oynanabilir kart yoksa null doner.</summary>
    public Card? ChooseCard(GameState state, Player player)
    {
        var playable = player.Hand.Where(c => GameEngine.CanPlay(state, c)).ToList();
        if (playable.Count == 0)
            return null;

        // Jokerleri en sona birak, aksiyon kartlarini one al
        return playable
            .OrderBy(c => c.IsWild ? 1 : 0)
            .ThenByDescending(c => c.Type is CardType.Skip or CardType.Reverse or CardType.DrawTwo ? 1 : 0)
            .ThenByDescending(c => c.Points)
            .First();
    }

    /// <summary>Joker oynarken elindeki en yaygin rengi secer; hic renkli karti yoksa rastgele secer.</summary>
    public CardColor ChooseColor(Player player)
    {
        var best = player.Hand
            .Where(c => !c.IsWild)
            .GroupBy(c => c.Color)
            .OrderByDescending(g => g.Count())
            .Select(g => (CardColor?)g.Key)
            .FirstOrDefault();

        if (best is not null)
            return best.Value;

        CardColor[] colors = [CardColor.Red, CardColor.Yellow, CardColor.Green, CardColor.Blue];
        return colors[_random.Next(colors.Length)];
    }
}
