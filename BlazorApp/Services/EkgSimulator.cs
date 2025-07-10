using Microsoft.AspNetCore.SignalR;
using BlazorApp.Hubs;

namespace BlazorApp.Services;

/// <summary>
/// BackgroundService that pushes a fake EKG signal (sine wave + noise)
/// to all connected SignalR clients ~100 times per second.
/// </summary>
public class EkgSimulator : BackgroundService
{
    private readonly IHubContext<EkgHub> _hubContext;
    private readonly Random _rand = new();

    public EkgSimulator(IHubContext<EkgHub> hubContext)
    {
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const double twoPi = Math.PI * 2;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        while (!stoppingToken.IsCancellationRequested)
        {
            // 1 Hz sine + 0-0.2 random noise
            double t = sw.Elapsed.TotalSeconds;
            double value = Math.Sin(t * twoPi) + (_rand.NextDouble() - 0.5) * 0.2;

            await _hubContext.Clients.All.SendAsync("ReceiveEkgValue", value, cancellationToken: stoppingToken);

            await Task.Delay(10, stoppingToken); // 100 Hz
        }
    }
}
