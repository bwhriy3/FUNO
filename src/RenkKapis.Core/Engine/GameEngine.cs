using RenkKapis.Core.Model;

namespace RenkKapis.Core.Engine;

/// <summary>
/// Oyunun kural motoru. Durum tasimaz; verilen GameState uzerinde islem yapar.
/// UI, ag katmani ve veritabanindan tamamen bagimsizdir.
/// </summary>
public static class GameEngine
{
    /// <summary>
    /// Yeni bir oyun baslatir: desteyi kurar, kartlari dagitir ve ilk karti acar.
    /// Basitlik icin ilk acilan kart bir sayi karti olana kadar cekilir;
    /// boylece oyun aksiyon karti efektiyle baslamaz.
    /// </summary>
    public static GameState StartNew(IEnumerable<Player> players, GameOptions? options = null, Random? random = null)
    {
        var playerList = players.ToList();
        if (playerList.Count is < 2 or > 10)
            throw new ArgumentException("Oyuncu sayisi 2 ile 10 arasinda olmalidir.", nameof(players));

        options ??= GameOptions.Default;
        var deck = new Deck(random);
        var state = new GameState(playerList, deck, options);

        foreach (var player in playerList)
            for (int i = 0; i < options.StartingHandSize; i++)
                player.AddCard(deck.Draw());

        Card first;
        var skipped = new List<Card>();
        while ((first = deck.Draw()).Type != CardType.Number)
            skipped.Add(first);

        // Atlanan aksiyon/joker kartlari desteye geri karistirilir
        foreach (var card in skipped)
            deck.Discard(card);

        deck.Discard(first);
        state.ActiveColor = first.Color;

        return state;
    }

    /// <summary>Verilen kart su anki duruma gore oynanabilir mi?</summary>
    public static bool CanPlay(GameState state, Card card)
    {
        var top = state.TopCard
            ?? throw new InvalidOperationException("Atilan yigin bos; oyun baslatilmamis.");

        // Birikmis ceza varsa sadece ayni tip ceza karti oynanabilir
        if (state.PendingDrawCount > 0)
        {
            return state.PendingDrawType switch
            {
                CardType.DrawTwo => state.Options.StackDrawTwo && card.Type == CardType.DrawTwo,
                CardType.WildDrawFour => state.Options.StackDrawFour && card.Type == CardType.WildDrawFour,
                _ => false
            };
        }

        if (card.IsWild)
            return true;

        if (card.Color == state.ActiveColor)
            return true;

        // Ayni sayi veya ayni aksiyon tipi
        if (card.Type == CardType.Number && top.Type == CardType.Number)
            return card.Value == top.Value;

        return card.Type == top.Type;
    }

    /// <summary>Oyuncunun elinde oynanabilir kart var mi?</summary>
    public static bool HasPlayableCard(GameState state, Player player)
        => player.Hand.Any(c => CanPlay(state, c));

    /// <summary>
    /// Sirasi gelen oyuncu bir kart oynar.
    /// Joker oynaniyorsa <paramref name="chosenColor"/> verilmelidir.
    /// </summary>
    public static PlayResult PlayCard(GameState state, Card card, CardColor? chosenColor = null)
    {
        if (state.IsFinished)
            return PlayResult.Fail("Oyun zaten bitti.");

        var player = state.CurrentPlayer;

        if (!player.HasCard(card))
            return PlayResult.Fail("Bu kart oyuncunun elinde yok.");

        if (!CanPlay(state, card))
            return PlayResult.Fail("Bu kart su an oynanamaz.");

        if (card.IsWild)
        {
            if (chosenColor is null or CardColor.Wild)
                return PlayResult.Fail("Joker oynarken bir renk secilmelidir.");
        }

        player.RemoveCard(card);
        state.Deck.Discard(card);
        state.ActiveColor = card.IsWild ? chosenColor!.Value : card.Color;

        // "Tek!" demeyi unutma cezasi
        if (state.Options.EnforceUnoCall && player.CardCount == 1 && !player.HasCalledUno)
        {
            for (int i = 0; i < state.Options.UnoPenaltyCards; i++)
                player.AddCard(state.Deck.Draw());
        }

        if (player.CardCount > 1)
            player.HasCalledUno = false;

        if (player.CardCount == 0)
        {
            ScoreRound(state, player);
            state.Finish(player);
            return PlayResult.Ok($"{player.Name} eli bitirdi!");
        }

        ApplyCardEffect(state, card);
        return PlayResult.Ok($"{player.Name} oynadi: {card}");
    }

    /// <summary>
    /// Sirasi gelen oyuncu kart ceker. Birikmis ceza varsa cezanin tamami cekilir
    /// ve sira gecer; ceza yoksa tek kart cekilir.
    /// </summary>
    public static DrawResult DrawCard(GameState state)
    {
        if (state.IsFinished)
            return new DrawResult([], false, "Oyun zaten bitti.");

        var player = state.CurrentPlayer;

        if (state.PendingDrawCount > 0)
        {
            var penalty = new List<Card>();
            for (int i = 0; i < state.PendingDrawCount; i++)
            {
                var drawn = state.Deck.Draw();
                player.AddCard(drawn);
                penalty.Add(drawn);
            }

            int count = state.PendingDrawCount;
            state.PendingDrawCount = 0;
            state.PendingDrawType = null;
            player.HasCalledUno = false;
            state.AdvanceTurn();

            return new DrawResult(penalty, false, $"{player.Name} {count} ceza karti cekti.");
        }

        var card = state.Deck.Draw();
        player.AddCard(card);
        player.HasCalledUno = false;

        bool playable = state.Options.PlayDrawnCard && CanPlay(state, card);
        if (!playable)
            state.AdvanceTurn();

        return new DrawResult([card], playable, $"{player.Name} kart cekti.");
    }

    /// <summary>Oyuncu "Tek!" der. Ancak son kartini oynamadan hemen once gecerlidir.</summary>
    public static bool CallUno(GameState state, Player player)
    {
        if (player.CardCount != 2)
            return false;

        player.HasCalledUno = true;
        return true;
    }

    /// <summary>Oynanan kartin efektini uygular ve sirayi ilerletir.</summary>
    private static void ApplyCardEffect(GameState state, Card card)
    {
        switch (card.Type)
        {
            case CardType.Skip:
                state.AdvanceTurn(2);
                break;

            case CardType.Reverse:
                if (state.Players.Count == 2 && state.Options.ReverseActsAsSkipInTwoPlayerGame)
                {
                    // Iki kisilik oyunda Ters, Pas gibi davranir: sira yine ayni oyuncuda kalir
                    state.AdvanceTurn(2);
                }
                else
                {
                    state.ReverseDirection();
                    state.AdvanceTurn();
                }
                break;

            case CardType.DrawTwo:
                state.PendingDrawCount += 2;
                state.PendingDrawType = CardType.DrawTwo;
                state.AdvanceTurn();
                break;

            case CardType.WildDrawFour:
                state.PendingDrawCount += 4;
                state.PendingDrawType = CardType.WildDrawFour;
                state.AdvanceTurn();
                break;

            default:
                state.AdvanceTurn();
                break;
        }
    }

    /// <summary>El bitince kazanana, digerlerinin elindeki kartlarin puani eklenir.</summary>
    private static void ScoreRound(GameState state, Player winner)
    {
        winner.Score += state.Players.Where(p => p != winner).Sum(p => p.HandPoints);
    }
}
