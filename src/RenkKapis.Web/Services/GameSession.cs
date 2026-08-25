using RenkKapis.Core.Ai;
using RenkKapis.Core.Engine;
using RenkKapis.Core.Model;

namespace RenkKapis.Web.Services;

/// <summary>
/// Tek kisilik oyun oturumu: bir insan oyuncu + botlar.
/// Kural mantigi Core'daki GameEngine'e aittir; bu sinif yalnizca
/// sirayi orkestre eder ve arayuzu haberdar eder.
/// </summary>
public sealed class GameSession
{
    private SimpleBot _bot = new();
    private readonly List<string> _log = new();

    /// <summary>Arayuzun yeniden cizilmesi gerektiginde tetiklenir.</summary>
    public event Func<Task>? StateChanged;

    public GameState? State { get; private set; }
    public Player? Human { get; private set; }

    /// <summary>Bot sirasi islerken true; bu sirada insan girdisi kilitlenir.</summary>
    public bool IsBotThinking { get; private set; }

    /// <summary>Joker oynandi, oyuncunun renk secmesi bekleniyor.</summary>
    public Card? PendingWildCard { get; private set; }

    /// <summary>Cekilen kart oynanabilir; oyuncu "oyna" veya "pas" diyebilir.</summary>
    public Card? DrawnPlayableCard { get; private set; }

    public IReadOnlyList<string> Log => _log;

    public bool IsHumanTurn =>
        State is { IsFinished: false } s && s.CurrentPlayer == Human && !IsBotThinking;

    /// <summary>Bot hamleleri arasindaki bekleme (ms). Testte 0 verilebilir.</summary>
    public int BotDelayMs { get; set; } = 900;

    public async Task StartNewGameAsync(string humanName, int botCount, int? seed = null)
    {
        var random = seed is null ? new Random() : new Random(seed.Value);
        _bot = new SimpleBot(random);
        _log.Clear();

        var human = new Player(string.IsNullOrWhiteSpace(humanName) ? "Sen" : humanName.Trim());
        var players = new List<Player> { human };

        string[] botNames = ["Ali", "Veli", "Ayse", "Fatma", "Mehmet"];
        for (int i = 0; i < botCount; i++)
            players.Add(new Player(botNames[i % botNames.Length], isBot: true));

        Human = human;
        State = GameEngine.StartNew(players, GameOptions.Default, random);
        PendingWildCard = null;
        DrawnPlayableCard = null;

        AddLog($"Oyun basladi. Ilk kart: {Describe(State.TopCard!)}");
        await NotifyAsync();
        await RunBotTurnsAsync();
    }

    /// <summary>Insan oyuncu elindeki bir karti oynamak ister.</summary>
    public async Task PlayCardAsync(Card card)
    {
        if (State is null || Human is null || !IsHumanTurn)
            return;

        if (!GameEngine.CanPlay(State, card))
        {
            AddLog("Bu kart su an oynanamaz.");
            await NotifyAsync();
            return;
        }

        // Joker ise once renk secimi gerekiyor
        if (card.IsWild)
        {
            PendingWildCard = card;
            await NotifyAsync();
            return;
        }

        await ExecuteHumanPlayAsync(card, null);
    }

    /// <summary>Renk secim penceresinden bir renk secildi.</summary>
    public async Task ChooseColorAsync(CardColor color)
    {
        if (PendingWildCard is null)
            return;

        var card = PendingWildCard;
        PendingWildCard = null;
        await ExecuteHumanPlayAsync(card, color);
    }

    /// <summary>Renk secimi iptal edildi, kart elde kalir.</summary>
    public async Task CancelColorChoiceAsync()
    {
        PendingWildCard = null;
        await NotifyAsync();
    }

    private async Task ExecuteHumanPlayAsync(Card card, CardColor? color)
    {
        if (State is null || Human is null)
            return;

        DrawnPlayableCard = null;

        int before = Human.CardCount;
        var result = GameEngine.PlayCard(State, card, color);

        if (!result.Success)
        {
            AddLog(result.Message);
            await NotifyAsync();
            return;
        }

        string colorNote = color is not null ? $" (renk: {Localize(color.Value)})" : "";
        AddLog($"{Human.Name}: {Describe(card)}{colorNote}");

        // "Tek!" demeyi unutma cezasi uygulandiysa kullaniciya bildir
        if (before - 1 == 1 && Human.CardCount > 1)
            AddLog($"{Human.Name} 'Tek!' demeyi unuttu, 2 ceza karti cekti.");

        await NotifyAsync();
        await RunBotTurnsAsync();
    }

    /// <summary>Insan oyuncu desteden kart ceker.</summary>
    public async Task DrawCardAsync()
    {
        if (State is null || Human is null || !IsHumanTurn)
            return;

        DrawnPlayableCard = null;
        var result = GameEngine.DrawCard(State);
        AddLog(result.Message);

        if (result.CanPlayDrawnCard)
        {
            // Sira hala insanda: cektigi karti oynayabilir veya pas gecebilir
            DrawnPlayableCard = result.DrawnCards[0];
            await NotifyAsync();
            return;
        }

        await NotifyAsync();
        await RunBotTurnsAsync();
    }

    /// <summary>Cekilen karti oynamayip sirayi devretmek.</summary>
    public async Task PassAfterDrawAsync()
    {
        if (State is null || Human is null || DrawnPlayableCard is null)
            return;

        DrawnPlayableCard = null;
        State.AdvanceTurn();
        AddLog($"{Human.Name} pas gecti.");

        await NotifyAsync();
        await RunBotTurnsAsync();
    }

    /// <summary>Insan oyuncu "Tek!" der.</summary>
    public async Task CallUnoAsync()
    {
        if (State is null || Human is null)
            return;

        if (GameEngine.CallUno(State, Human))
        {
            AddLog($"{Human.Name}: TEK!");
            await NotifyAsync();
        }
    }

    /// <summary>Sira insana gelene veya oyun bitene kadar botlari oynatir.</summary>
    private async Task RunBotTurnsAsync()
    {
        if (State is null)
            return;

        IsBotThinking = true;

        while (!State.IsFinished && State.CurrentPlayer.IsBot)
        {
            await NotifyAsync();

            if (BotDelayMs > 0)
                await Task.Delay(BotDelayMs);

            var player = State.CurrentPlayer;
            var choice = _bot.ChooseCard(State, player);

            if (choice is null)
            {
                var draw = GameEngine.DrawCard(State);
                AddLog(draw.Message);

                if (draw.CanPlayDrawnCard)
                    PlayAsBot(player, draw.DrawnCards[0]);

                continue;
            }

            PlayAsBot(player, choice);
        }

        IsBotThinking = false;

        if (State.IsFinished)
            AddLog($"Oyun bitti! Kazanan: {State.Winner!.Name}");

        await NotifyAsync();
    }

    private void PlayAsBot(Player player, Card card)
    {
        if (State is null)
            return;

        // Bot son kartina dusecekse "Tek!" demeyi unutmaz
        if (player.CardCount == 2)
        {
            GameEngine.CallUno(State, player);
            AddLog($"{player.Name}: TEK!");
        }

        CardColor? color = card.IsWild ? _bot.ChooseColor(player) : null;
        var result = GameEngine.PlayCard(State, card, color);

        string colorNote = color is not null ? $" (renk: {Localize(color.Value)})" : "";
        AddLog(result.Success
            ? $"{player.Name}: {Describe(card)}{colorNote}"
            : $"{player.Name} HATA: {result.Message}");
    }

    private void AddLog(string message)
    {
        _log.Insert(0, message);
        if (_log.Count > 40)
            _log.RemoveAt(_log.Count - 1);
    }

    private Task NotifyAsync() => StateChanged?.Invoke() ?? Task.CompletedTask;

    public static string Localize(CardColor color) => color switch
    {
        CardColor.Red => "Kirmizi",
        CardColor.Yellow => "Sari",
        CardColor.Green => "Yesil",
        CardColor.Blue => "Mavi",
        _ => "Renksiz"
    };

    /// <summary>Karti Turkce olarak tanimlar.</summary>
    public static string Describe(Card card) => card.Type switch
    {
        CardType.Number => $"{Localize(card.Color)} {card.Value}",
        CardType.Skip => $"{Localize(card.Color)} Pas",
        CardType.Reverse => $"{Localize(card.Color)} Ters",
        CardType.DrawTwo => $"{Localize(card.Color)} +2",
        CardType.Wild => "Joker",
        CardType.WildDrawFour => "Joker +4",
        _ => card.ToString()
    };
}
