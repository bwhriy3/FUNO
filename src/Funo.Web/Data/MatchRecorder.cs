using Microsoft.EntityFrameworkCore;

namespace Funo.Web.Data;

/// <summary>One participant's outcome, ready to be persisted.</summary>
public sealed record MatchSeatResult(string PlayerName, bool IsBot, bool IsWinner);

/// <summary>
/// Persists finished games. Registered as a singleton so both the scoped
/// single-player <c>GameSession</c> and the singleton-owned multiplayer
/// <c>GameRoom</c>s can record results; it creates a short-lived
/// <see cref="FunoDbContext"/> per call via the factory rather than holding
/// one open for its own lifetime.
/// </summary>
public sealed class MatchRecorder
{
    private readonly IDbContextFactory<FunoDbContext> _factory;

    public MatchRecorder(IDbContextFactory<FunoDbContext> factory) => _factory = factory;

    public async Task RecordAsync(bool wasMultiplayer, IReadOnlyList<MatchSeatResult> seats)
    {
        var winner = seats.FirstOrDefault(s => s.IsWinner);
        if (winner is null)
            return;

        await using var db = await _factory.CreateDbContextAsync();

        var match = new Match
        {
            WasMultiplayer = wasMultiplayer,
            WinnerName = winner.PlayerName,
            Seats = seats.Select(s => new MatchSeat
            {
                PlayerName = s.PlayerName,
                IsBot = s.IsBot,
                IsWinner = s.IsWinner
            }).ToList()
        };

        db.Matches.Add(match);
        await db.SaveChangesAsync();
    }
}
