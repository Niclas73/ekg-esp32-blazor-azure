using Microsoft.JSInterop;

namespace BlazorApp;

public static class InteropHelper
{
    private static Action<int, int>? _updateCallback;

    public static void Register(Action<int, int> callback)
    {
        _updateCallback = callback;
    }

    [JSInvokable]
    public static Task UpdateMetrics(int hr, int hrv)
    {
        _updateCallback?.Invoke(hr, hrv);
        return Task.CompletedTask;
    }
}
