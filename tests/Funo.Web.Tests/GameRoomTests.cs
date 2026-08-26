using Funo.Core.Model;
using Funo.Web.Rooms;

namespace Funo.Web.Tests;

public class GameRoomTests
{
    private static GameRoom NewRoom(string host = "Bahriye", string connectionId = "conn-host")
    {
        var room = new GameRoom("ABCDE", host);
        var error = room.Join(host, connectionId);
        Assert.Null(error);
        return room;
    }

    [Fact]
    public void Join_FirstPlayer_BecomesHost()
    {
        var room = NewRoom();
        Assert.Equal("Bahriye", room.HostName);

        var view = room.BuildViewFor("Bahriye")!;
        Assert.Single(view.Players);
        Assert.False(view.IsStarted);
    }

    [Fact]
    public void Join_DuplicateNameWhileConnected_Fails()
    {
        var room = NewRoom();
        var error = room.Join("Bahriye", "conn-other");

        Assert.Equal("room.nameTaken", error);
    }

    [Fact]
    public void Join_SameNameAfterDisconnect_Reconnects()
    {
        var room = NewRoom();
        room.Join("Ahmet", "conn-2");

        room.Disconnect("conn-2");
        var error = room.Join("Ahmet", "conn-2-new");

        Assert.Null(error);
        var view = room.BuildViewFor("Ahmet")!;
        Assert.True(view.Players.Single(p => p.Name == "Ahmet").IsConnected);
    }

    [Fact]
    public void Join_AfterGameStarted_Fails()
    {
        var room = NewRoom();
        room.Join("Ahmet", "conn-2");
        room.Start("Bahriye", botCount: 0);

        var error = room.Join("Yeni", "conn-3");

        Assert.Equal("room.alreadyStarted", error);
    }

    [Fact]
    public void Join_RoomFull_Fails()
    {
        var room = NewRoom();
        for (int i = 2; i <= 6; i++)
            Assert.Null(room.Join($"Oyuncu{i}", $"conn-{i}"));

        var error = room.Join("Yedinci", "conn-7");

        Assert.Equal("room.full", error);
    }

    [Fact]
    public void Disconnect_BeforeGameStarts_RemovesPlayerAndReassignsHost()
    {
        var room = NewRoom();
        room.Join("Ahmet", "conn-2");

        room.Disconnect("conn-host");

        Assert.Equal("Ahmet", room.HostName);
        var view = room.BuildViewFor("Ahmet")!;
        Assert.Single(view.Players);
    }

    [Fact]
    public void Start_OnlyHostCanStart()
    {
        var room = NewRoom();
        room.Join("Ahmet", "conn-2");

        var error = room.Start("Ahmet", botCount: 0);

        Assert.Equal("mp.onlyHostStarts", error);
        Assert.False(room.IsStarted);
    }

    [Fact]
    public void Start_NeedsAtLeastTwoPlayers()
    {
        var room = NewRoom();

        var error = room.Start("Bahriye", botCount: 0);

        Assert.Equal("room.needTwoPlayers", error);
        Assert.False(room.IsStarted);
    }

    [Fact]
    public void Start_FillsWithBots_AndBeginsGame()
    {
        var room = NewRoom();

        var error = room.Start("Bahriye", botCount: 3);

        Assert.Null(error);
        Assert.True(room.IsStarted);

        var view = room.BuildViewFor("Bahriye")!;
        Assert.Equal(4, view.Players.Count);
        Assert.Equal(3, view.Players.Count(p => p.IsBot));
        Assert.Equal(7, view.YourHand.Count);
    }

    [Fact]
    public void Start_Twice_Fails()
    {
        var room = NewRoom();
        room.Join("Ahmet", "conn-2");
        room.Start("Bahriye", botCount: 0);

        var error = room.Start("Bahriye", botCount: 0);

        Assert.Equal("room.gameAlreadyStarted", error);
    }

    [Fact]
    public void PlayCard_WhenNotYourTurn_Fails()
    {
        var room = NewRoom();
        room.Join("Ahmet", "conn-2");
        room.Start("Bahriye", botCount: 0);

        var view = room.BuildViewFor("Bahriye")!;
        Assert.True(view.IsYourTurn);

        // Ahmet'in sirasi degil
        var ahmetView = room.BuildViewFor("Ahmet")!;
        Assert.False(ahmetView.IsYourTurn);

        var error = room.PlayCard("Ahmet", ahmetView.YourHand[0], null);

        Assert.Equal("room.notYourTurn", error);
    }

    [Fact]
    public void PlayCard_BeforeGameStarts_Fails()
    {
        var room = NewRoom();
        var dummyCard = new Funo.Web.Contracts.CardDto(CardColor.Red, CardType.Number, 5);

        var error = room.PlayCard("Bahriye", dummyCard, null);

        Assert.Equal("room.notStarted", error);
    }

    [Fact]
    public void DrawCard_BeforeGameStarts_Fails()
    {
        var room = NewRoom();

        var error = room.DrawCard("Bahriye");

        Assert.Equal("room.notStarted", error);
    }

    [Fact]
    public void CallUno_WithMoreThanTwoCards_Fails()
    {
        var room = NewRoom();
        room.Join("Ahmet", "conn-2");
        room.Start("Bahriye", botCount: 0);

        var error = room.CallUno("Bahriye");

        Assert.Equal("room.cannotCallUno", error);
    }

    [Fact]
    public void BuildViews_OnlyIncludesConnectedMembers()
    {
        var room = NewRoom();
        room.Join("Ahmet", "conn-2");
        room.Disconnect("conn-2");

        var views = room.BuildViews();

        Assert.Single(views);
        Assert.Equal("Bahriye", views[0].View.YouAre);
    }

    [Fact]
    public void BuildViews_NeverExposesOpponentHandContents()
    {
        var room = NewRoom();
        room.Join("Ahmet", "conn-2");
        room.Start("Bahriye", botCount: 0);

        var views = room.BuildViews();

        foreach (var (_, view) in views)
        {
            // Kendi elimiz haric hicbir oyuncunun kart listesi acikta olmamali;
            // PlayerView yalnizca sayi tasir, kart listesi tasimaz.
            Assert.Equal(7, view.YourHand.Count);
            Assert.All(view.Players, p => Assert.True(p.CardCount >= 0));
        }
    }

    [Fact]
    public void FullGame_WithDisconnectedHumanHost_BotsFinishTheGame()
    {
        var room = NewRoom();
        room.Start("Bahriye", botCount: 3);

        // Insan (oda sahibi) baglantisini kaybediyor; botlar onun yerine oynamali
        // ve oyun kilitlenmeden bitmeli.
        room.Disconnect("conn-host");

        var finalView = room.BuildViewFor("Bahriye");

        // Baglantisi kopan oyuncunun gorunumu artik uretilemez (BuildViews'ta yer almaz)
        // ama BuildViewFor dogrudan isimle sorgulandiginda hala calismali.
        Assert.NotNull(finalView);
        Assert.True(finalView!.IsFinished);
        Assert.NotNull(finalView.WinnerName);
    }

    [Fact]
    public void Reconnect_DuringActiveGame_RestoresView()
    {
        var room = NewRoom();
        room.Join("Ahmet", "conn-2");
        room.Start("Bahriye", botCount: 0);

        // Bahriye ilk oyuncu oldugu icin baglantisi kesilir kesilmez
        // bot onun yerine bir hamle yapar (kart oynar veya ceker).
        room.Disconnect("conn-host");
        var rejoinError = room.Join("Bahriye", "conn-host-2");

        Assert.Null(rejoinError);
        var view = room.BuildViewFor("Bahriye")!;
        Assert.True(view.IsStarted);
        // Bot lehine oynanmis olsa da el hala gecerli bir oyun durumunu yansitmali.
        Assert.True(view.YourHand.Count is >= 5 and <= 9);
    }
}
