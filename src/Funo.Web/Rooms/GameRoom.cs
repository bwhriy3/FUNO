using Funo.Core.Ai;
using Funo.Core.Engine;
using Funo.Core.Model;
using Funo.Web.Contracts;
using Funo.Web.Localization;

namespace Funo.Web.Rooms;

/// <summary>Odadaki bir katilimci: domain oyuncusu + baglanti bilgisi.</summary>
public sealed class RoomMember
{
    public required Player Player { get; init; }

    /// <summary>Aktif SignalR baglanti kimligi. Kopmussa null.</summary>
    public string? ConnectionId { get; set; }

    public bool IsConnected => ConnectionId is not null;

    /// <summary>Baglantisi kopan oyuncu adina bot oynar.</summary>
    public bool IsBotControlled => Player.IsBot || !IsConnected;
}

/// <summary>
/// Cok oyunculu bir oyun odasi. Tum durum degisiklikleri kilit altinda yapilir;
/// ayni anda birden fazla istemciden gelen istekler yarismasin diye.
/// </summary>
public sealed class GameRoom
{
    private readonly object _lock = new();
    private readonly List<RoomMember> _members = new();
    private readonly List<LogEntry> _log = new();
    private readonly SimpleBot _bot = new();

    private GameState? _state;
    private Card? _drawnPlayableCard;
    private string? _drawnPlayableFor;

    public GameRoom(string code, string hostName)
    {
        Code = code;
        HostName = hostName;
    }

    public string Code { get; }
    public string HostName { get; private set; }
    public DateTimeOffset LastActivity { get; private set; } = DateTimeOffset.UtcNow;

    public bool IsStarted => _state is not null;

    /// <summary>Oyuncu odaya katilir veya kopmus baglantisini geri alir.</summary>
    /// <returns>Basarisizsa hata mesaji, basariliysa null.</returns>
    public string? Join(string playerName, string connectionId)
    {
        lock (_lock)
        {
            Touch();

            var existing = _members.FirstOrDefault(
                m => string.Equals(m.Player.Name, playerName, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                // Ayni isimle tekrar baglanma: yalnizca baglanti kopmussa izin ver
                if (existing.IsConnected)
                    return "room.nameTaken";

                existing.ConnectionId = connectionId;
                AddLog("log.reconnected", existing.Player.Name);
                return null;
            }

            if (_state is not null)
                return "room.alreadyStarted";

            if (_members.Count >= 6)
                return "room.full";

            _members.Add(new RoomMember
            {
                Player = new Player(playerName),
                ConnectionId = connectionId
            });

            AddLog("log.joined", playerName);
            return null;
        }
    }

    /// <summary>Baglantisi kopan uyeyi isaretler. Oyun basladiysa yerine bot oynar.</summary>
    public void Disconnect(string connectionId)
    {
        lock (_lock)
        {
            var member = _members.FirstOrDefault(m => m.ConnectionId == connectionId);
            if (member is null)
                return;

            member.ConnectionId = null;
            Touch();

            if (_state is null)
            {
                // Oyun baslamadiysa oyuncuyu tamamen cikar
                _members.Remove(member);
                AddLog("log.left", member.Player.Name);

                if (HostName == member.Player.Name && _members.Count > 0)
                {
                    HostName = _members[0].Player.Name;
                    AddLog("log.newHost", HostName);
                }
            }
            else
            {
                AddLog("log.botTookOver", member.Player.Name);
            }
        }
    }

    public bool IsEmpty
    {
        get { lock (_lock) { return _members.Count == 0 || _members.All(m => !m.IsConnected); } }
    }

    /// <summary>Oyunu baslatir. Yalnizca oda sahibi ve en az 2 oyuncu ile.</summary>
    public string? Start(string playerName, int botCount)
    {
        lock (_lock)
        {
            Touch();

            if (_state is not null)
                return "room.gameAlreadyStarted";

            if (!string.Equals(playerName, HostName, StringComparison.OrdinalIgnoreCase))
                return "mp.onlyHostStarts";

            // Eksik oyuncu varsa bot ile tamamla
            string[] botNames = ["Bot-Ali", "Bot-Veli", "Bot-Ayse", "Bot-Fatma"];
            for (int i = 0; i < botCount && _members.Count < 6; i++)
            {
                _members.Add(new RoomMember
                {
                    Player = new Player(botNames[i % botNames.Length], isBot: true)
                });
            }

            if (_members.Count < 2)
                return "room.needTwoPlayers";

            _state = GameEngine.StartNew(_members.Select(m => m.Player), GameOptions.Default);
            AddLog("log.gameStarted", Strings.Token.Card(_state.TopCard!));
            return null;
        }
    }

    public string? PlayCard(string playerName, CardDto cardDto, CardColor? chosenColor)
    {
        lock (_lock)
        {
            Touch();

            if (_state is null)
                return "room.notStarted";

            if (!IsTurnOf(playerName))
                return "room.notYourTurn";

            var card = cardDto.ToCard();
            var player = _state.CurrentPlayer;

            // Oyuncu son kartina dusuyorsa ve "Tek!" demediyse ceza motorda uygulanir
            int before = player.CardCount;
            var result = GameEngine.PlayCard(_state, card, chosenColor);

            if (!result.Success)
                return result.Message;

            _drawnPlayableCard = null;
            _drawnPlayableFor = null;

            if (chosenColor is not null)
                AddLog("log.playedColor", player.Name, Strings.Token.Card(card), Strings.Token.Color(chosenColor.Value));
            else
                AddLog("log.played", player.Name, Strings.Token.Card(card));

            if (before - 1 == 1 && player.CardCount > 1)
                AddLog("log.unoForgot", player.Name, _state.Options.UnoPenaltyCards.ToString());

            if (_state.IsFinished)
                AddLog("log.finished", _state.Winner!.Name);

            return null;
        }
    }

    public string? DrawCard(string playerName)
    {
        lock (_lock)
        {
            Touch();

            if (_state is null)
                return "room.notStarted";

            if (!IsTurnOf(playerName))
                return "room.notYourTurn";

            int pendingBefore = _state.PendingDrawCount;
            var result = GameEngine.DrawCard(_state);

            if (pendingBefore > 0)
                AddLog("log.drewPenalty", playerName, pendingBefore.ToString());
            else
                AddLog("log.drew", playerName);

            if (result.CanPlayDrawnCard)
            {
                _drawnPlayableCard = result.DrawnCards[0];
                _drawnPlayableFor = playerName;
                return null;
            }

            _drawnPlayableCard = null;
            _drawnPlayableFor = null;
            return null;
        }
    }

    public string? PassAfterDraw(string playerName)
    {
        lock (_lock)
        {
            Touch();

            if (_state is null || _drawnPlayableCard is null)
                return "room.nothingToPass";

            if (!IsTurnOf(playerName) || _drawnPlayableFor != playerName)
                return "room.notYourTurn";

            _drawnPlayableCard = null;
            _drawnPlayableFor = null;
            _state.AdvanceTurn();
            AddLog("log.passed", playerName);
            return null;
        }
    }

    public string? CallUno(string playerName)
    {
        lock (_lock)
        {
            Touch();

            if (_state is null)
                return "room.notStarted";

            var member = FindMember(playerName);
            if (member is null)
                return "room.playerNotFound";

            if (!GameEngine.CallUno(_state, member.Player))
                return "room.cannotCallUno";

            AddLog("log.uno", playerName);
            return null;
        }
    }

    /// <summary>Odadaki her bagli oyuncu icin kendi ozel gorunumunu uretir.</summary>
    public IReadOnlyList<(string ConnectionId, GameView View)> BuildViews()
    {
        lock (_lock)
        {
            return _members
                .Where(m => m.IsConnected)
                .Select(m => (m.ConnectionId!, BuildViewFor(m)))
                .ToList();
        }
    }

    public GameView? BuildViewFor(string playerName)
    {
        lock (_lock)
        {
            var member = FindMember(playerName);
            return member is null ? null : BuildViewFor(member);
        }
    }

    private GameView BuildViewFor(RoomMember me)
    {
        var players = _members
            .Select(m => new PlayerView(
                m.Player.Name,
                m.Player.CardCount,
                m.Player.IsBot,
                m.IsConnected,
                m.Player.HasCalledUno))
            .ToList();

        // Oyun baslamadiysa lobi gorunumu doner
        if (_state is null)
        {
            return new GameView(
                Code, me.Player.Name, false, false, null, players, null, false, 1,
                CardColor.Wild, null, 0, 0, [], [], null, HostName, _log.Take(20).ToList());
        }

        bool myTurn = _state.CurrentPlayer == me.Player && !_state.IsFinished;

        // KRITIK: yalnizca kendi elimiz gonderilir, rakiplerinki sayi olarak tasinir
        var hand = me.Player.Hand.Select(CardDto.From).ToList();
        var playable = me.Player.Hand
            .Select(c => myTurn && GameEngine.CanPlay(_state, c))
            .ToList();

        var drawn = _drawnPlayableFor == me.Player.Name && _drawnPlayableCard is not null
            ? CardDto.From(_drawnPlayableCard)
            : null;

        return new GameView(
            Code,
            me.Player.Name,
            true,
            _state.IsFinished,
            _state.Winner?.Name,
            players,
            _state.CurrentPlayer.Name,
            myTurn,
            _state.Direction,
            _state.ActiveColor,
            _state.TopCard is null ? null : CardDto.From(_state.TopCard),
            _state.Deck.DrawPileCount,
            _state.PendingDrawCount,
            hand,
            playable,
            drawn,
            HostName,
            _log.Take(20).ToList());
    }

    /// <summary>
    /// Tek bir botun (veya baglantisi kopmus oyuncunun) tam sirasini oynar:
    /// ya bir kart oynar, ya da ceker (ve cektigi kart oynanabiliyorsa hemen oynar).
    /// Bilerek TEK bir sira ile sinirlidir; boylece cagiran taraf (GameHub) her
    /// bot hamlesi arasina bir bekleme ve yayin koyup insanlarin oyunu takip
    /// edebilmesini saglayabilir. Kilitlemeyi kendi icinde yapar.
    /// </summary>
    /// <returns>Bir bot hamlesi yapildiysa true; sira insanda veya oyun bittiyse false.</returns>
    public bool TryAdvanceOneBotTurn()
    {
        lock (_lock)
        {
            if (_state is null || _state.IsFinished)
                return false;

            var member = FindMember(_state.CurrentPlayer.Name);
            if (member is null || !member.IsBotControlled)
                return false;

            var player = _state.CurrentPlayer;
            var choice = _bot.ChooseCard(_state, player);

            if (choice is null)
            {
                int pendingBefore = _state.PendingDrawCount;
                var draw = GameEngine.DrawCard(_state);

                if (pendingBefore > 0)
                    AddLog("log.drewPenalty", player.Name, pendingBefore.ToString());
                else
                    AddLog("log.drew", player.Name);

                if (draw.CanPlayDrawnCard)
                    BotPlay(player, draw.DrawnCards[0]);
            }
            else
            {
                BotPlay(player, choice);
            }

            if (_state.IsFinished && (_log.Count == 0 || _log[0].Key != "log.finished"))
                AddLog("log.finished", _state.Winner!.Name);

            return true;
        }
    }

    private void BotPlay(Player player, Card card)
    {
        if (_state is null)
            return;

        if (player.CardCount == 2)
        {
            GameEngine.CallUno(_state, player);
            AddLog("log.uno", player.Name);
        }

        CardColor? color = card.IsWild ? _bot.ChooseColor(player) : null;
        var result = GameEngine.PlayCard(_state, card, color);

        if (!result.Success)
        {
            AddLog(result.Message);
            return;
        }

        if (color is not null)
            AddLog("log.playedColor", player.Name, Strings.Token.Card(card), Strings.Token.Color(color.Value));
        else
            AddLog("log.played", player.Name, Strings.Token.Card(card));
    }

    private bool IsTurnOf(string playerName) =>
        _state is { IsFinished: false } &&
        string.Equals(_state.CurrentPlayer.Name, playerName, StringComparison.OrdinalIgnoreCase);

    private RoomMember? FindMember(string playerName) =>
        _members.FirstOrDefault(
            m => string.Equals(m.Player.Name, playerName, StringComparison.OrdinalIgnoreCase));

    private void AddLog(string key, params string[] args)
    {
        _log.Insert(0, new LogEntry(key, args));
        if (_log.Count > 50)
            _log.RemoveAt(_log.Count - 1);
    }

    private void Touch() => LastActivity = DateTimeOffset.UtcNow;


}
