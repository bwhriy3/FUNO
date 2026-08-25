namespace Funo.Core.Model;

/// <summary>
/// Tek bir oyun kartini temsil eder. Degismezdir (immutable).
/// Number kartlarinda Value 0-9 arasindadir, diger tiplerde null'dir.
/// </summary>
public sealed record Card
{
    public CardColor Color { get; }
    public CardType Type { get; }
    public int? Value { get; }

    private Card(CardColor color, CardType type, int? value)
    {
        Color = color;
        Type = type;
        Value = value;
    }

    public static Card Number(CardColor color, int value)
    {
        if (color == CardColor.Wild)
            throw new ArgumentException("Sayi karti renksiz olamaz.", nameof(color));
        if (value is < 0 or > 9)
            throw new ArgumentOutOfRangeException(nameof(value), "Sayi karti degeri 0-9 olmalidir.");

        return new Card(color, CardType.Number, value);
    }

    public static Card Action(CardColor color, CardType type)
    {
        if (color == CardColor.Wild)
            throw new ArgumentException("Aksiyon karti renksiz olamaz.", nameof(color));
        if (type is not (CardType.Skip or CardType.Reverse or CardType.DrawTwo))
            throw new ArgumentException("Gecersiz aksiyon karti tipi.", nameof(type));

        return new Card(color, type, null);
    }

    public static Card Wild() => new(CardColor.Wild, CardType.Wild, null);

    public static Card WildDrawFour() => new(CardColor.Wild, CardType.WildDrawFour, null);

    /// <summary>Joker mi? (renk secimi gerektiren kartlar)</summary>
    public bool IsWild => Type is CardType.Wild or CardType.WildDrawFour;

    /// <summary>Oyun sonu puanlamasinda bu kartin degeri.</summary>
    public int Points => Type switch
    {
        CardType.Number => Value!.Value,
        CardType.Skip or CardType.Reverse or CardType.DrawTwo => 20,
        CardType.Wild or CardType.WildDrawFour => 50,
        _ => 0
    };

    public override string ToString() => Type switch
    {
        CardType.Number => $"{Color} {Value}",
        CardType.Wild => "Joker",
        CardType.WildDrawFour => "Joker+4",
        _ => $"{Color} {Type}"
    };
}
