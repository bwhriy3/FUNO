using Funo.Web.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Funo.Web.Tests;

/// <summary>
/// GameRoom and RoomManager now require a MatchRecorder to persist finished
/// games. Tests use a throwaway in-memory SQLite database so persistence
/// actually runs (catching real EF Core mapping mistakes) without touching
/// the file system or needing any external service.
/// </summary>
internal static class TestSupport
{
    public static MatchRecorder CreateRecorder()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<FunoDbContext>().UseSqlite(connection).Options;

        using (var db = new FunoDbContext(options))
            db.Database.EnsureCreated();

        return new MatchRecorder(new InMemoryDbContextFactory(options, connection));
    }

    /// <summary>Keeps the backing SQLite connection alive for the test's lifetime.</summary>
    private sealed class InMemoryDbContextFactory : IDbContextFactory<FunoDbContext>
    {
        private readonly DbContextOptions<FunoDbContext> _options;
        private readonly SqliteConnection _connection; // referenced so it isn't garbage-collected/closed

        public InMemoryDbContextFactory(DbContextOptions<FunoDbContext> options, SqliteConnection connection)
        {
            _options = options;
            _connection = connection;
        }

        public FunoDbContext CreateDbContext() => new(_options);
    }
}
