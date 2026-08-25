using RenkKapis.Core.Model;

namespace RenkKapis.Web.Contracts;

/// <summary>
/// Ag uzerinden tasinabilir kart temsili. Core'daki Card ozel kurucuya
/// sahip oldugu icin dogrudan serilestirilmez.
/// </summary>
public sealed record CardDto(CardColor Color, CardType Type, int? Value)
{
    public static CardDto From(Card card) => new(card.Color, card.Type, card.Value);

    /// <summary>DTO'yu tekrar domain kartina cevirir.</summary>
    public Card ToCard() => Type switch
    {
        CardType.Number => Card.Number(Color, Value ?? 0),
        CardType.Wild => Card.Wild(),
        CardType.WildDrawFour => Card.WildDrawFour(),
        _ => Card.Action(Color, Type)
    };
}

/// <summary>Bir oyuncunun herkese acik bilgileri. El icerigi ASLA burada yer almaz.</summary>
public sealed record PlayerView(
    string Name,
    int CardCount,
    bool IsBot,
    bool IsConnected,
    bool HasCalledUno);

/// <summary>
/// Tek bir oyuncuya ozel oyun gorunumu.
/// Her istemci yalnizca kendi elini gorur; rakiplerin elleri sayi olarak tasinir.
/// </summary>
public sealed record GameView(
    string RoomCode,
    string YouAre,
    bool IsStarted,
    bool IsFinished,
    string? WinnerName,
    IReadOnlyList<PlayerView> Players,
    string? CurrentPlayerName,
    bool IsYourTurn,
    int Direction,
    CardColor ActiveColor,
    CardDto? TopCard,
    int DrawPileCount,
    int PendingDrawCount,
    IReadOnlyList<CardDto> YourHand,
    IReadOnlyList<bool> YourPlayable,
    CardDto? DrawnPlayableCard,
    string? HostName,
    IReadOnlyList<string> Log);
