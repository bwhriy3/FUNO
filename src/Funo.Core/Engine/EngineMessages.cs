namespace Funo.Core.Engine;

/// <summary>
/// Motorun urettigi mesaj anahtarlari.
/// Cekirdek katman kullaniciya gosterilecek metni URETMEZ; yalnizca anahtar doner.
/// Ceviri sunum katmaninda yapilir, boylece ayni oyunda farkli dilleri
/// kullanan oyuncular desteklenebilir.
///
/// Message keys produced by the engine. The core layer never produces
/// display text - only keys. Translation happens in the presentation layer,
/// so players in the same game can use different languages.
/// </summary>
public static class EngineMessages
{
    public const string GameAlreadyFinished = "engine.gameAlreadyFinished";
    public const string CardNotInHand = "engine.cardNotInHand";
    public const string CardNotPlayable = "engine.cardNotPlayable";
    public const string ColorRequired = "engine.colorRequired";
    public const string PlayerFinished = "engine.playerFinished";
    public const string CardPlayed = "engine.cardPlayed";
    public const string CardDrawn = "engine.cardDrawn";
    public const string PenaltyDrawn = "engine.penaltyDrawn";
}
