namespace Funo.Web.Data;

/// <summary>One completed game, single-player or multiplayer.</summary>
public sealed class Match
{
    public int Id { get; set; }

    // DateTime (UTC), not DateTimeOffset: SQLite's EF Core provider cannot
    // translate ORDER BY on DateTimeOffset columns into SQL.
    public DateTime PlayedAtUtc { get; set; } = DateTime.UtcNow;

    public bool WasMultiplayer { get; set; }

    /// <summary>Denormalized for quick display without joining Seats.</summary>
    public required string WinnerName { get; set; }

    public List<MatchSeat> Seats { get; set; } = new();
}

/// <summary>One participant's seat in a finished match. Players are identified by name only - no accounts.</summary>
public sealed class MatchSeat
{
    public int Id { get; set; }

    public int MatchId { get; set; }
    public Match? Match { get; set; }

    public required string PlayerName { get; set; }
    public bool IsBot { get; set; }
    public bool IsWinner { get; set; }
}
