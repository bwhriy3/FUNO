using Funo.Core.Model;

namespace Funo.Core.Engine;

/// <summary>Kart oynama denemesinin sonucu.</summary>
public sealed record PlayResult(bool Success, string Message)
{
    public static PlayResult Ok(string message) => new(true, message);
    public static PlayResult Fail(string message) => new(false, message);
}

/// <summary>
/// Kart cekme sonucu.
/// <paramref name="CanPlayDrawnCard"/> true ise oyuncu cektigi karti ayni turda oynayabilir.
/// </summary>
public sealed record DrawResult(IReadOnlyList<Card> DrawnCards, bool CanPlayDrawnCard, string Message);
