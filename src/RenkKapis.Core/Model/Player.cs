namespace RenkKapis.Core.Model;

/// <summary>Bir oyuncuyu ve elindeki kartlari temsil eder.</summary>
public sealed class Player
{
    private readonly List<Card> _hand = new();

    public Player(string name, bool isBot = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Oyuncu adi bos olamaz.", nameof(name));

        Name = name;
        IsBot = isBot;
    }

    public string Name { get; }
    public bool IsBot { get; }

    /// <summary>Oyuncunun eli (salt okunur gorunum).</summary>
    public IReadOnlyList<Card> Hand => _hand;

    public int CardCount => _hand.Count;

    /// <summary>Oyuncu bu turda "Tek!" dedi mi? Kart oynandiginda sifirlanir.</summary>
    public bool HasCalledUno { get; set; }

    /// <summary>Bu el icin toplanan ceza/puan.</summary>
    public int Score { get; set; }

    public void AddCard(Card card) => _hand.Add(card);

    public void AddCards(IEnumerable<Card> cards) => _hand.AddRange(cards);

    public bool RemoveCard(Card card) => _hand.Remove(card);

    public bool HasCard(Card card) => _hand.Contains(card);

    /// <summary>Elindeki kartlarin toplam puani (el sonu hesaplamasi icin).</summary>
    public int HandPoints => _hand.Sum(c => c.Points);

    public override string ToString() => $"{Name} ({_hand.Count} kart)";
}
