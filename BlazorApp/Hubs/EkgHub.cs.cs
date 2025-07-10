using Microsoft.AspNetCore.SignalR;

namespace BlazorApp.Hubs;

public class EkgHub : Hub
{
    // No server-invokable methods yet – we only broadcast from server to clients.
}
