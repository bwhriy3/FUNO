using RenkKapis.Core.Ai;
using RenkKapis.Core.Engine;
using RenkKapis.Core.Model;

// Sprint 1 dogrulamasi: arayuz olmadan 4 bot birbirine karsi tam bir oyun oynar.
// Amac kural motorunun bastan sona tutarli calistigini gormek.

int seed = args.Length > 0 && int.TryParse(args[0], out var s) ? s : Random.Shared.Next();
var random = new Random(seed);

Console.WriteLine($"=== RenkKapis - Bot Simulasyonu (seed: {seed}) ===\n");

var players = new List<Player>
{
    new("Bot-Ali", isBot: true),
    new("Bot-Veli", isBot: true),
    new("Bot-Ayse", isBot: true),
    new("Bot-Fatma", isBot: true)
};

var state = GameEngine.StartNew(players, GameOptions.Default, random);
var bot = new SimpleBot(random);

Console.WriteLine($"Ilk kart: {state.TopCard}\n");

int turn = 0;
const int maxTurns = 2000;   // sonsuz donguye karsi guvenlik

while (!state.IsFinished && turn < maxTurns)
{
    turn++;
    var player = state.CurrentPlayer;
    string prefix = $"[{turn,3}] {player.Name,-10}";

    // Ceza birikmisse ve karsilik veremiyorsa cekmek zorunda
    var choice = bot.ChooseCard(state, player);

    if (choice is null)
    {
        var draw = GameEngine.DrawCard(state);
        Console.WriteLine($"{prefix} {draw.Message}");

        if (draw.CanPlayDrawnCard)
        {
            var drawn = draw.DrawnCards[0];
            PlayWithUnoCheck(state, bot, player, drawn, $"{prefix} cektigini oynadi:");
        }
        continue;
    }

    PlayWithUnoCheck(state, bot, player, choice, $"{prefix} oynadi:");
}

Console.WriteLine();

if (state.IsFinished)
{
    Console.WriteLine($"*** KAZANAN: {state.Winner!.Name} ({turn} hamlede) ***\n");
    Console.WriteLine("El sonu durumu:");
    foreach (var p in state.Players)
        Console.WriteLine($"  {p.Name,-10} {p.CardCount,2} kart, el puani {p.HandPoints,3}, skor {p.Score}");
}
else
{
    Console.WriteLine($"!!! Oyun {maxTurns} hamlede bitmedi - kural motorunda dongu olabilir.");
}

Console.WriteLine($"\nDeste: {state.Deck.DrawPileCount} kart, atilan yigin: {state.Deck.DiscardPileCount} kart");

static void PlayWithUnoCheck(GameState state, SimpleBot bot, Player player, Card card, string prefix)
{
    // Bot son kartina dusecekse "Tek!" demeyi unutmaz
    if (player.CardCount == 2)
    {
        GameEngine.CallUno(state, player);
        Console.WriteLine($"{new string(' ', 6)}{player.Name} TEK! dedi");
    }

    CardColor? color = card.IsWild ? bot.ChooseColor(player) : null;
    var result = GameEngine.PlayCard(state, card, color);

    string colorNote = color is not null ? $" (renk: {color})" : "";
    Console.WriteLine(result.Success
        ? $"{prefix} {card}{colorNote}  -> {player.CardCount} kart kaldi"
        : $"{prefix} HATA: {result.Message}");
}
