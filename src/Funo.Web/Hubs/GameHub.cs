using Funo.Core.Model;
using Funo.Web.Contracts;
using Funo.Web.Rooms;
using Microsoft.AspNetCore.SignalR;

namespace Funo.Web.Hubs;

/// <summary>
/// Cok oyunculu oyunun ag katmani.
/// Istemciler yalnizca niyetlerini bildirir; tum kural kararlari sunucuda verilir.
/// Bu sayede istemci tarafi kurcalanarak hile yapilamaz.
/// </summary>
public sealed class GameHub : Hub
{
    /// <summary>Bot hamleleri arasindaki bekleme. Insanlarin oyunu takip edebilmesi icin.</summary>
    private const int BotMoveDelayMs = 900;

    private readonly RoomManager _rooms;
    private readonly ILogger<GameHub> _logger;

    public GameHub(RoomManager rooms, ILogger<GameHub> logger)
    {
        _rooms = rooms;
        _logger = logger;
    }

    // Baglantiya ait bilgiyi hub uzerinde saklariz; her cagride tekrar gonderilmesin.
    private string? RoomCode
    {
        get => Context.Items.TryGetValue("room", out var v) ? v as string : null;
        set => Context.Items["room"] = value;
    }

    private string? PlayerName
    {
        get => Context.Items.TryGetValue("player", out var v) ? v as string : null;
        set => Context.Items["player"] = value;
    }

    /// <summary>Yeni oda kurar ve kurucuyu odaya alir. Oda kodunu doner.</summary>
    public async Task<string> CreateRoom(string playerName)
    {
        playerName = Normalize(playerName);

        var room = _rooms.Create(playerName);
        var error = room.Join(playerName, Context.ConnectionId);

        if (error is not null)
            throw new HubException(error);

        RoomCode = room.Code;
        PlayerName = playerName;

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
        await BroadcastAsync(room);

        _logger.LogInformation("Oda kuruldu: {Code} ({Player})", room.Code, playerName);
        return room.Code;
    }

    /// <summary>Var olan odaya katilir. Ayni isimle kopmus baglanti varsa geri alir.</summary>
    public async Task JoinRoom(string code, string playerName)
    {
        playerName = Normalize(playerName);

        var room = _rooms.Find(code) ?? throw new HubException("room.notFound");
        var error = room.Join(playerName, Context.ConnectionId);

        if (error is not null)
            throw new HubException(error);

        RoomCode = room.Code;
        PlayerName = playerName;

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
        await BroadcastAsync(room);
    }

    public Task StartGame(int botCount) => ExecuteAsync(room => room.Start(PlayerName!, botCount));

    public Task PlayCard(CardDto card, CardColor? chosenColor) =>
        ExecuteAsync(room => room.PlayCard(PlayerName!, card, chosenColor));

    public Task DrawCard() => ExecuteAsync(room => room.DrawCard(PlayerName!));

    public Task PassAfterDraw() => ExecuteAsync(room => room.PassAfterDraw(PlayerName!));

    public Task CallUno() => ExecuteAsync(room => room.CallUno(PlayerName!));

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (RoomCode is not null && _rooms.Find(RoomCode) is { } room)
        {
            room.Disconnect(Context.ConnectionId);
            await BroadcastAsync(room);

            if (room.IsEmpty && !room.IsStarted)
                _rooms.Remove(room.Code);
            else
                await DriveBotsAsync(room);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Ortak akis: odayi bul, islemi yap, hata varsa cagirana bildir, herkese yayinla,
    /// sonra sira bota (veya baglantisi kopmus oyuncuya) gectigi surece botlari
    /// TEK TEK, aralarina bekleme koyarak oynat. Boylece 3 bota karsi oynarken
    /// oyuncu, botlarin hangi karti oynadigini gorebiliyor - hepsi tek seferde
    /// birden bitivermiyor.
    /// </summary>
    private async Task ExecuteAsync(Func<GameRoom, string?> action)
    {
        if (RoomCode is null || PlayerName is null)
            throw new HubException("room.joinFirst");

        var room = _rooms.Find(RoomCode) ?? throw new HubException("room.notFound");
        var error = action(room);

        if (error is not null)
        {
            // Hata yalnizca islemi yapana gonderilir, digerlerini ilgilendirmez
            await Clients.Caller.SendAsync("Error", error);
            return;
        }

        // Once kendi hamlemizi goster
        await BroadcastAsync(room);

        // Sonra botlarin sirasini, her birini goze gorunur sekilde, tek tek oynat
        await DriveBotsAsync(room);
    }

    /// <summary>
    /// Sira bot(a benzer) bir oyuncuda oldugu surece tek tek hamle yaptirir;
    /// her hamleden sonra durumu yayinlar ve bir sonrakine gecmeden once bekler.
    /// </summary>
    private async Task DriveBotsAsync(GameRoom room)
    {
        while (room.TryAdvanceOneBotTurn())
        {
            await BroadcastAsync(room);
            await Task.Delay(BotMoveDelayMs);
        }
    }

    /// <summary>
    /// Her oyuncuya YALNIZCA kendi gorunumunu gonderir.
    /// Tum durumu gruba yayinlamak rakiplerin elini sizdirirdi.
    /// </summary>
    private async Task BroadcastAsync(GameRoom room)
    {
        foreach (var (connectionId, view) in room.BuildViews())
            await Clients.Client(connectionId).SendAsync("GameUpdated", view);
    }

    private static string Normalize(string name)
    {
        name = (name ?? "").Trim();

        if (name.Length == 0)
            throw new HubException("room.nameEmpty");

        return name.Length > 16 ? name[..16] : name;
    }
}
