using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Rakawatch.Services;

public sealed class HardwareSamplerService(
    HardwareMonitorService monitor,
    ILogger<HardwareSamplerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMs = int.TryParse(Environment.GetEnvironmentVariable("SAMPLE_INTERVAL_MS"), out var value)
            ? value
            : 1000;

        if (intervalMs <= 0)
            intervalMs = 1000;

        TryUpdate();

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));

        while (await timer.WaitForNextTickAsync(stoppingToken))
            TryUpdate();
    }

    private void TryUpdate()
    {
        try
        {
            monitor.Update();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update hardware snapshot.");
        }
    }
}