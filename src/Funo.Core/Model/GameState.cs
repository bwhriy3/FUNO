namespace Funo.Core.Model;

/// <summary>Oyunun o anki tum durumunu tutar. Kural mantigi GameEngine'dedir.</summary>
public sealed class GameState
{
    private readonly List<Player> _players;

    public GameState(IEnumerable<Player> players, Deck deck, GameOptions options)
    {
        _players = players.ToList();
        Deck = deck;
        Options = options;
    }

    public IReadOnlyList<Player> Players => _players;
    public Deck Deck { get; }
    public GameOptions Options { get; }

    /// <summary>Sirasi gelen oyuncunun indeksi.</summary>
    public int CurrentPlayerIndex { get; set; }

    /// <summary>Oyun yonu: +1 saat yonu, -1 tersi.</summary>
    public int Direction { get; set; } = 1;

    /// <summary>
    /// Su an gecerli olan renk. Joker oynandiginda oyuncunun sectigi renk
    /// buraya yazilir; bu yuzden ustteki kartin renginden farkli olabilir.
    /// </summary>
    public CardColor ActiveColor { get; set; }

    /// <summary>Birikmis ceza kart sayisi (+2 / +4 zinciri).</summary>
    public int PendingDrawCount { get; set; }

    /// <summary>Birikmis cezanin tipi (DrawTwo veya WildDrawFour). Ceza yoksa null.</summary>
    public CardType? PendingDrawType { get; set; }

    public bool IsFinished { get; private set; }
    public Player? Winner { get; private set; }

    public Player CurrentPlayer => _players[CurrentPlayerIndex];

    public Card? TopCard => Deck.TopDiscard;

    /// <summary>Siradaki oyuncunun indeksini (yonu dikkate alarak) hesaplar.</summary>
    public int GetNextPlayerIndex(int step = 1)
    {
        int count = _players.Count;
        return ((CurrentPlayerIndex + Direction * step) % count + count) % count;
    }

    public void AdvanceTurn(int step = 1) => CurrentPlayerIndex = GetNextPlayerIndex(step);

    public void ReverseDirection() => Direction = -Direction;

    public void Finish(Player winner)
    {
        IsFinished = true;
        Winner = winner;
    }
}
