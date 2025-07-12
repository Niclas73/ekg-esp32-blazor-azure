using Microsoft.AspNetCore.SignalR;
using BlazorApp.Hubs;

namespace BlazorApp.Services;

/// <summary>
/// Mall-baserad EKG-generator: 48 diskreta punkter (= 0,192 s @ 250 Hz).
/// Upprepas med RR 0.33–0.37 s  → puls ≈ 165–180 bpm.
/// R-toppen ligger ALLTID på ett sample ⇒ ingen aliasing.
/// HRV skapas med ±1 % respiratorisk modul + ±8 ms slump.
/// Signal mappas över hela RR-intervallet med linjär interpolation.
/// </summary>
public sealed class EkgSimulator : BackgroundService
{
    private readonly IHubContext<EkgHub> _hub;
    private readonly Random _rnd = new();
    public EkgSimulator(IHubContext<EkgHub> hub) => _hub = hub;

    // 48-punkts EKG-signal (lead I)
    private static readonly double[] Template =
    {
        // P-våg (6 p)
         0.00, 0.03, 0.06, 0.06, 0.03, 0.00,
        // PR-isolin
         0.00, 0.00, 0.00,
        // QRS-komplex
        -0.14, -0.20, 0.90, 1.60, -0.06, -0.08,
        // ST-isolin
         0.00, 0.00, 0.00,
        // T-våg (10 p)
         0.02, 0.06, 0.11, 0.16, 0.20, 0.20, 0.16, 0.11, 0.06, 0.02,
        // Baslinje till 48 totalt
         0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
         0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
         0, 0, 0
    };

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        const int Fs = 250;               // Hz
        const double dt = 1.0 / Fs;       // 4 ms
        int tplLen = Template.Length;

        double t = 0.0;
        double nextBeat = 0.35;

        int samplesThisBeat = (int)(0.35 / dt);
        int sampleIndex = 0;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("▶️  Mallbaserad EKG-generator startad");
        Console.ResetColor();

        while (!token.IsCancellationRequested)
        {
            if (t >= nextBeat)
            {
                double jitter = (_rnd.NextDouble() - 0.5) * 0.008; // ±8 ms
                double rsa = 0.01 * Math.Sin(2 * Math.PI * 0.2 * t); // ±1 %
                double rr = 0.35 + jitter + rsa; // ≈ 0.33–0.37 s

                nextBeat = t + rr;
                samplesThisBeat = (int)(rr / dt);
                sampleIndex = 0;
            }

            // Interpolera template över RR-intervallet
            double pos = sampleIndex * (double)tplLen / samplesThisBeat;
            int i = (int)Math.Floor(pos);
            double frac = pos - i;

            double y = i + 1 < tplLen
                ? (1 - frac) * Template[i] + frac * Template[i + 1]
                : Template[^1]; // sista värdet

            double sample = y + (_rnd.NextDouble() - 0.5) * 0.008;

            await _hub.Clients.All.SendAsync("ReceiveEkgValue", sample, token);

            await Task.Delay(4, token); // 250 Hz
            t += dt;
            sampleIndex++;
        }
    }
}
