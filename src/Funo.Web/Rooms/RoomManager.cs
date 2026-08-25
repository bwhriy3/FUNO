using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Funo.Web.Rooms;

/// <summary>
/// Tum aktif odalari tutar. Singleton olarak kaydedilir.
/// </summary>
public sealed class RoomManager
{
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Karisikligi onlemek icin benzer gorunen harfler (I, O) disarida birakildi.</summary>
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public GameRoom Create(string hostName)
    {
        // Cakisma ihtimaline karsi bos bir kod bulunana kadar dene
        for (int attempt = 0; attempt < 20; attempt++)
        {
            string code = GenerateCode();
            var room = new GameRoom(code, hostName);

            if (_rooms.TryAdd(code, room))
                return room;
        }

        throw new InvalidOperationException("Bos oda kodu uretilemedi.");
    }

    public GameRoom? Find(string code) =>
        code is not null && _rooms.TryGetValue(code, out var room) ? room : null;

    public void Remove(string code) => _rooms.TryRemove(code, out _);

    /// <summary>Bos ve uzun suredir hareketsiz odalari temizler.</summary>
    public int CleanupIdleRooms(TimeSpan maxIdle)
    {
        int removed = 0;
        var threshold = DateTimeOffset.UtcNow - maxIdle;

        foreach (var (code, room) in _rooms)
        {
            if (room.IsEmpty && room.LastActivity < threshold && _rooms.TryRemove(code, out _))
                removed++;
        }

        return removed;
    }

    private static string GenerateCode()
    {
        Span<char> buffer = stackalloc char[5];
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];

        return new string(buffer);
    }
}
