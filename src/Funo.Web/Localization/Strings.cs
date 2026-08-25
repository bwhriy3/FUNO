using Funo.Core.Model;

namespace Funo.Web.Localization;

/// <summary>Supported interface languages / Desteklenen arayuz dilleri.</summary>
public enum Lang
{
    Tr,
    En
}

/// <summary>
/// A structured log entry: a message key plus its arguments.
/// Text is produced only at render time, so two players in the same game
/// can read the same event in different languages.
///
/// Yapisal kayit girdisi: mesaj anahtari + parametreleri.
/// Metin yalnizca ekrana cizilirken uretilir; boylece ayni oyundaki iki oyuncu
/// ayni olayi farkli dillerde okuyabilir.
/// </summary>
public sealed record LogEntry(string Key, params string[] Args);

/// <summary>
/// Central translation table. Every user-visible string lives here;
/// no display text is hard-coded anywhere else.
/// </summary>
public static class Strings
{
    private static readonly Dictionary<string, (string Tr, string En)> Table = new()
    {
        // ---- Setup / lobby ----
        ["app.tagline"] = ("UNO benzeri kart oyunu", "A UNO-style card game"),
        ["setup.name"] = ("Adin", "Your name"),
        ["setup.namePlaceholder"] = ("Sen", "You"),
        ["setup.opponents"] = ("Rakip sayisi", "Opponents"),
        ["setup.bots"] = ("{0} bot", "{0} bot(s)"),
        ["setup.start"] = ("Oyunu Basla", "Start Game"),
        ["setup.singlePlayer"] = ("Tek Kisilik", "Single Player"),
        ["setup.multiPlayer"] = ("Cok Oyunculu", "Multiplayer"),

        // ---- Multiplayer lobby ----
        ["mp.title"] = ("Cok Oyunculu", "Multiplayer"),
        ["mp.createRoom"] = ("Oda Kur", "Create Room"),
        ["mp.joinRoom"] = ("Odaya Katil", "Join Room"),
        ["mp.roomCode"] = ("Oda kodu", "Room code"),
        ["mp.roomCodeIs"] = ("Oda kodu: {0}", "Room code: {0}"),
        ["mp.shareCode"] = ("Bu kodu arkadaslarinla paylas", "Share this code with your friends"),
        ["mp.waiting"] = ("Oyuncular bekleniyor…", "Waiting for players…"),
        ["mp.playersInRoom"] = ("Odadaki oyuncular", "Players in room"),
        ["mp.host"] = ("oda sahibi", "host"),
        ["mp.you"] = ("sen", "you"),
        ["mp.disconnected"] = ("baglanti kopuk", "disconnected"),
        ["mp.fillWithBots"] = ("Bot ekle", "Add bots"),
        ["mp.startGame"] = ("Oyunu Baslat", "Start Game"),
        ["mp.onlyHostStarts"] = ("Oyunu yalnizca oda sahibi baslatabilir.", "Only the host can start the game."),
        ["mp.leave"] = ("Odadan Ayril", "Leave Room"),
        ["mp.connecting"] = ("Baglaniyor…", "Connecting…"),
        ["mp.connectionLost"] = ("Baglanti koptu, yeniden deneniyor…", "Connection lost, reconnecting…"),
        ["mp.back"] = ("Geri", "Back"),

        // ---- Table ----
        ["table.deck"] = ("Deste", "Deck"),
        ["table.topCard"] = ("Ustteki kart", "Top card"),
        ["table.activeColor"] = ("Gecerli renk", "Active color"),
        ["table.penalty"] = ("+{0} ceza!", "+{0} penalty!"),
        ["table.cards"] = ("{0} kart", "{0} cards"),
        ["table.yourTurn"] = ("Sira sende", "Your turn"),
        ["table.mustDraw"] = ("— {0} kart cekmelisin", "— you must draw {0} cards"),
        ["table.thinking"] = ("{0} dusunuyor…", "{0} is thinking…"),
        ["table.waitingFor"] = ("{0} oynuyor…", "Waiting for {0}…"),
        ["table.winner"] = ("Kazanan: {0}", "Winner: {0}"),
        ["table.youWon"] = ("Kazandin!", "You won!"),
        ["table.drawCard"] = ("Kart cek", "Draw card"),
        ["table.drawN"] = ("{0} kart cek", "Draw {0} cards"),
        ["table.pass"] = ("Pas gec", "Pass"),
        ["table.drawnHint"] = ("{0} cektin — oynayabilirsin", "You drew {0} — you may play it"),
        ["table.callUno"] = ("TEK! de", "Call UNO!"),
        ["table.newGame"] = ("Yeni oyun", "New game"),
        ["table.gameLog"] = ("Oyun kaydi", "Game log"),
        ["table.chooseColor"] = ("Renk sec", "Choose a color"),
        ["table.cancel"] = ("Vazgec", "Cancel"),
        ["table.unoBadge"] = ("TEK!", "UNO!"),

        // ---- Colors ----
        ["color.Red"] = ("Kirmizi", "Red"),
        ["color.Yellow"] = ("Sari", "Yellow"),
        ["color.Green"] = ("Yesil", "Green"),
        ["color.Blue"] = ("Mavi", "Blue"),
        ["color.Wild"] = ("Renksiz", "Wild"),

        // ---- Card names ----
        ["card.number"] = ("{0} {1}", "{0} {1}"),
        ["card.skip"] = ("{0} Pas", "{0} Skip"),
        ["card.reverse"] = ("{0} Ters", "{0} Reverse"),
        ["card.drawTwo"] = ("{0} +2", "{0} +2"),
        ["card.wild"] = ("Joker", "Wild"),
        ["card.wildDrawFour"] = ("Joker +4", "Wild +4"),
        ["card.faceDown"] = ("Kapali kart", "Face-down card"),

        // ---- Game log events ----
        ["log.gameStarted"] = ("Oyun basladi. Ilk kart: {0}", "Game started. First card: {0}"),
        ["log.played"] = ("{0}: {1}", "{0}: {1}"),
        ["log.playedColor"] = ("{0}: {1} (renk: {2})", "{0}: {1} (color: {2})"),
        ["log.drew"] = ("{0} kart cekti.", "{0} drew a card."),
        ["log.drewPenalty"] = ("{0}, {1} ceza karti cekti.", "{0} drew {1} penalty cards."),
        ["log.passed"] = ("{0} pas gecti.", "{0} passed."),
        ["log.uno"] = ("{0}: TEK!", "{0}: UNO!"),
        ["log.unoForgot"] = ("{0} 'Tek!' demeyi unuttu, {1} ceza karti cekti.",
                             "{0} forgot to call UNO and drew {1} penalty cards."),
        ["log.finished"] = ("Oyun bitti! Kazanan: {0}", "Game over! Winner: {0}"),
        ["log.joined"] = ("{0} odaya katildi.", "{0} joined the room."),
        ["log.left"] = ("{0} odadan ayrildi.", "{0} left the room."),
        ["log.reconnected"] = ("{0} yeniden baglandi.", "{0} reconnected."),
        ["log.botTookOver"] = ("{0} baglantisi koptu, yerine bot oynuyor.",
                               "{0} disconnected - a bot is playing for them."),
        ["log.newHost"] = ("Yeni oda sahibi: {0}", "New host: {0}"),

        // ---- Engine errors ----
        ["engine.gameAlreadyFinished"] = ("Oyun zaten bitti.", "The game is already over."),
        ["engine.cardNotInHand"] = ("Bu kart elinde yok.", "That card is not in your hand."),
        ["engine.cardNotPlayable"] = ("Bu kart su an oynanamaz.", "That card cannot be played right now."),
        ["engine.colorRequired"] = ("Joker oynarken bir renk secilmelisin.", "You must choose a color for a wild card."),
        ["engine.playerFinished"] = ("Oyuncu elini bitirdi!", "Player emptied their hand!"),
        ["engine.cardPlayed"] = ("Kart oynandi.", "Card played."),
        ["engine.cardDrawn"] = ("Kart cekildi.", "Card drawn."),
        ["engine.penaltyDrawn"] = ("Ceza kartlari cekildi.", "Penalty cards drawn."),

        // ---- Room errors ----
        ["room.notFound"] = ("Oda bulunamadi.", "Room not found."),
        ["room.nameTaken"] = ("Bu isim odada zaten kullaniliyor.", "That name is already taken in this room."),
        ["room.alreadyStarted"] = ("Oyun basladi, yeni oyuncu katilamaz.", "The game has started; no new players can join."),
        ["room.full"] = ("Oda dolu (en fazla 6 oyuncu).", "The room is full (6 players maximum)."),
        ["room.notYourTurn"] = ("Sira sende degil.", "It is not your turn."),
        ["room.notStarted"] = ("Oyun henuz baslamadi.", "The game has not started yet."),
        ["room.needTwoPlayers"] = ("Oyun icin en az 2 oyuncu gerekiyor.", "At least 2 players are required."),
        ["room.gameAlreadyStarted"] = ("Oyun zaten basladi.", "The game has already started."),
        ["room.playerNotFound"] = ("Oyuncu bulunamadi.", "Player not found."),
        ["room.cannotCallUno"] = ("Simdi 'Tek!' diyemezsin.", "You cannot call UNO right now."),
        ["room.nothingToPass"] = ("Pas gecilecek bir durum yok.", "There is nothing to pass on."),
        ["room.joinFirst"] = ("Once bir odaya katilmalisin.", "You must join a room first."),
        ["room.nameEmpty"] = ("Oyuncu adi bos olamaz.", "Player name cannot be empty."),
        ["room.codeEmpty"] = ("Oda kodu bos olamaz.", "Room code cannot be empty."),
    };

    /// <summary>Translates a key. Unknown keys are returned as-is so nothing silently disappears.</summary>
    public static string Get(string key, Lang lang)
    {
        if (!Table.TryGetValue(key, out var entry))
            return key;

        return lang == Lang.En ? entry.En : entry.Tr;
    }

    /// <summary>Translates a key and fills in {0}, {1}, … placeholders.</summary>
    public static string Get(string key, Lang lang, params object[] args)
    {
        string template = Get(key, lang);
        return args.Length == 0 ? template : string.Format(template, args);
    }

    public static string Color(CardColor color, Lang lang) => Get($"color.{color}", lang);

    /// <summary>Human-readable card name in the requested language.</summary>
    public static string Card(Card card, Lang lang) => card.Type switch
    {
        CardType.Number => Get("card.number", lang, Color(card.Color, lang), card.Value!.Value),
        CardType.Skip => Get("card.skip", lang, Color(card.Color, lang)),
        CardType.Reverse => Get("card.reverse", lang, Color(card.Color, lang)),
        CardType.DrawTwo => Get("card.drawTwo", lang, Color(card.Color, lang)),
        CardType.Wild => Get("card.wild", lang),
        CardType.WildDrawFour => Get("card.wildDrawFour", lang),
        _ => card.ToString()
    };

    /// <summary>
    /// Renders a structured log entry. Arguments may be plain text (player names,
    /// numbers) or tokens produced by <see cref="Token"/>, which are translated too.
    /// </summary>
    public static string Log(LogEntry entry, Lang lang)
    {
        object[] args = entry.Args.Select(a => (object)ResolveArg(a, lang)).ToArray();
        return Get(entry.Key, lang, args);
    }

    private static string ResolveArg(string arg, Lang lang)
    {
        // "card:Red:Number:5" / "color:Blue" seklindeki jetonlar cevrilir
        if (arg.StartsWith("card:", StringComparison.Ordinal))
        {
            var parts = arg.Split(':');
            var color = Enum.Parse<CardColor>(parts[1]);
            var type = Enum.Parse<CardType>(parts[2]);
            int? value = parts[3].Length == 0 ? null : int.Parse(parts[3]);

            return type switch
            {
                CardType.Number => Get("card.number", lang, Color(color, lang), value!.Value),
                CardType.Skip => Get("card.skip", lang, Color(color, lang)),
                CardType.Reverse => Get("card.reverse", lang, Color(color, lang)),
                CardType.DrawTwo => Get("card.drawTwo", lang, Color(color, lang)),
                CardType.Wild => Get("card.wild", lang),
                CardType.WildDrawFour => Get("card.wildDrawFour", lang),
                _ => arg
            };
        }

        if (arg.StartsWith("color:", StringComparison.Ordinal))
            return Color(Enum.Parse<CardColor>(arg[6..]), lang);

        return Table.ContainsKey(arg) ? Get(arg, lang) : arg;
    }

    /// <summary>Builds translatable log arguments / Cevrilebilir kayit parametreleri uretir.</summary>
    public static class Token
    {
        public static string Card(Card card) =>
            $"card:{card.Color}:{card.Type}:{card.Value?.ToString() ?? ""}";

        public static string Color(CardColor color) => $"color:{color}";
    }
}
