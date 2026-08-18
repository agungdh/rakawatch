using Microsoft.Extensions.Hosting;

namespace Rakawatch.Services;

public sealed class HardwareSamplerService(HardwareMonitorService monitor) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMs = int.TryParse(Environment.GetEnvironmentVariable("SAMPLE_INTERVAL_MS"), out var value)
            ? value
            : 1000;

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));

        while (await timer.WaitForNextTickAsync(stoppingToken))
            monitor.Update();
    }
}