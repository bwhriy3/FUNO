namespace RenkKapis.Core.Model;

/// <summary>
/// Oyunun yerel kural varyasyonlari. UNO'nun evden eve degisen kurallari
/// burada acikca sabitlenir; boylece motorun davranisi belirsiz kalmaz.
/// </summary>
public sealed record GameOptions
{
    /// <summary>Oyun basinda her oyuncuya dagitilan kart sayisi.</summary>
    public int StartingHandSize { get; init; } = 7;

    /// <summary>+2 kartinin ustune baska bir +2 atilarak ceza biriktirilebilir mi?</summary>
    public bool StackDrawTwo { get; init; } = true;

    /// <summary>+4 kartinin ustune baska bir +4 atilarak ceza biriktirilebilir mi?</summary>
    public bool StackDrawFour { get; init; } = false;

    /// <summary>Desteden cekilen kart oynanabiliyorsa ayni turda oynanabilir mi?</summary>
    public bool PlayDrawnCard { get; init; } = true;

    /// <summary>"Tek!" demeyi unutan oyuncu ceza kart ceker mi?</summary>
    public bool EnforceUnoCall { get; init; } = true;

    /// <summary>"Tek!" demeyi unutmanin cezasi (kart sayisi).</summary>
    public int UnoPenaltyCards { get; init; } = 2;

    /// <summary>
    /// Iki kisilik oyunda Ters karti Pas gibi davranir (sira yine ayni oyuncuya doner).
    /// </summary>
    public bool ReverseActsAsSkipInTwoPlayerGame { get; init; } = true;

    /// <summary>Varsayilan kural seti.</summary>
    public static GameOptions Default => new();
}
