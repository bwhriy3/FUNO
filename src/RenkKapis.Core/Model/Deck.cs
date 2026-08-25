namespace RenkKapis.Core.Model;

/// <summary>
/// Cekme destesi ve atilan kart yiginini yonetir.
/// Cekme destesi bitince, en ustteki kart haric atilanlar karistirilip yeni deste olur.
/// </summary>
public sealed class Deck
{
    private readonly List<Card> _drawPile = new();
    private readonly List<Card> _discardPile = new();
    private readonly Random _random;

    public Deck(Random? random = null)
    {
        _random = random ?? new Random();
        BuildFullDeck();
        Shuffle();
    }

    public int DrawPileCount => _drawPile.Count;
    public int DiscardPileCount => _discardPile.Count;

    /// <summary>Atilan yiginin en ustundeki kart. Yigin bossa null.</summary>
    public Card? TopDiscard => _discardPile.Count > 0 ? _discardPile[^1] : null;

    /// <summary>Standart 108 kartlik desteyi olusturur.</summary>
    private void BuildFullDeck()
    {
        CardColor[] colors = [CardColor.Red, CardColor.Yellow, CardColor.Green, CardColor.Blue];

        foreach (var color in colors)
        {
            // Her renkte bir tane 0, ikiser tane 1-9
            _drawPile.Add(Card.Number(color, 0));
            for (int value = 1; value <= 9; value++)
            {
                _drawPile.Add(Card.Number(color, value));
                _drawPile.Add(Card.Number(color, value));
            }

            // Her renkte ikiser tane Pas, Ters, +2
            foreach (var type in new[] { CardType.Skip, CardType.Reverse, CardType.DrawTwo })
            {
                _drawPile.Add(Card.Action(color, type));
                _drawPile.Add(Card.Action(color, type));
            }
        }

        // 4 Joker + 4 Joker+4
        for (int i = 0; i < 4; i++)
        {
            _drawPile.Add(Card.Wild());
            _drawPile.Add(Card.WildDrawFour());
        }
    }

    /// <summary>Cekme destesini Fisher-Yates ile karistirir.</summary>
    private void Shuffle()
    {
        for (int i = _drawPile.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (_drawPile[i], _drawPile[j]) = (_drawPile[j], _drawPile[i]);
        }
    }

    /// <summary>
    /// Desteden bir kart ceker. Deste bossa atilan yigin (en ust kart haric)
    /// karistirilip yeni cekme destesi yapilir.
    /// </summary>
    /// <exception cref="InvalidOperationException">Karistirilacak kart da kalmadiysa.</exception>
    public Card Draw()
    {
        if (_drawPile.Count == 0)
            RecycleDiscardPile();

        var card = _drawPile[^1];
        _drawPile.RemoveAt(_drawPile.Count - 1);
        return card;
    }

    /// <summary>Bir karti atilan yiginin ustune koyar.</summary>
    public void Discard(Card card) => _discardPile.Add(card);

    private void RecycleDiscardPile()
    {
        if (_discardPile.Count <= 1)
            throw new InvalidOperationException("Cekilecek kart kalmadi: hem deste hem atilan yigin tukendi.");

        var top = _discardPile[^1];
        _discardPile.RemoveAt(_discardPile.Count - 1);

        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
        _discardPile.Add(top);

        Shuffle();
    }
}
