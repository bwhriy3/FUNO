namespace Funo.Web.Rooms;

/// <summary>
/// Periodically sweeps <see cref="RoomManager"/> for rooms that are both
/// empty (everyone disconnected) and idle past the threshold, so abandoned
/// rooms don't accumulate in memory for the lifetime of the process.
/// </summary>
public sealed class RoomCleanupService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan IdleThreshold = TimeSpan.FromMinutes(30);

    private readonly RoomManager _rooms;
    private readonly ILogger<RoomCleanupService> _logger;

    public RoomCleanupService(RoomManager rooms, ILogger<RoomCleanupService> logger)
    {
        _rooms = rooms;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                int removed = _rooms.CleanupIdleRooms(IdleThreshold);
                if (removed > 0)
                    _logger.LogInformation("Cleaned up {Count} idle room(s).", removed);
            }
            catch (Exception ex)
            {
                // A cleanup failure should never take the whole app down.
                _logger.LogError(ex, "Room cleanup pass failed.");
            }

            try
            {
                await Task.Delay(CheckInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
