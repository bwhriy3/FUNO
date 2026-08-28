using Microsoft.EntityFrameworkCore;

namespace Funo.Web.Data;

public sealed class FunoDbContext : DbContext
{
    public FunoDbContext(DbContextOptions<FunoDbContext> options) : base(options) { }

    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchSeat> MatchSeats => Set<MatchSeat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Match>()
            .HasMany(m => m.Seats)
            .WithOne(s => s.Match)
            .HasForeignKey(s => s.MatchId);
    }
}
