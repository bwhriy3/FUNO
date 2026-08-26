using Funo.Web.Rooms;

namespace Funo.Web.Tests;

public class RoomManagerTests
{
    [Fact]
    public void Create_ReturnsFindableRoom()
    {
        var manager = new RoomManager();

        var room = manager.Create("Bahriye");
        var found = manager.Find(room.Code);

        Assert.NotNull(found);
        Assert.Same(room, found);
    }

    [Fact]
    public void Create_GeneratesUniqueCodes()
    {
        var manager = new RoomManager();

        var codes = Enumerable.Range(0, 50)
            .Select(_ => manager.Create("Host").Code)
            .ToList();

        Assert.Equal(codes.Count, codes.Distinct().Count());
    }

    [Fact]
    public void Find_UnknownCode_ReturnsNull()
    {
        var manager = new RoomManager();

        Assert.Null(manager.Find("ZZZZZ"));
    }

    [Fact]
    public void Remove_DeletesRoom()
    {
        var manager = new RoomManager();
        var room = manager.Create("Bahriye");

        manager.Remove(room.Code);

        Assert.Null(manager.Find(room.Code));
    }

    [Fact]
    public void CleanupIdleRooms_RemovesOnlyEmptyAndIdle()
    {
        var manager = new RoomManager();
        var idleEmpty = manager.Create("Host1");
        idleEmpty.Join("Host1", "conn-1");
        idleEmpty.Disconnect("conn-1"); // artik bos

        var activeRoom = manager.Create("Host2");
        activeRoom.Join("Host2", "conn-2"); // hala bagli, bos degil

        int removed = manager.CleanupIdleRooms(TimeSpan.Zero);

        Assert.Equal(1, removed);
        Assert.Null(manager.Find(idleEmpty.Code));
        Assert.NotNull(manager.Find(activeRoom.Code));
    }
}
