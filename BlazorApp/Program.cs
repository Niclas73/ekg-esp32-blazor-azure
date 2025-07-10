using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using BlazorApp.Data;
using BlazorApp.Hubs;          // ← add
using BlazorApp.Services;      // ← add

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// 1.  Register framework services
// ─────────────────────────────────────────────────────────────
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// ─────────────────────────────────────────────────────────────
// 2.  Add realtime + simulator services
// ─────────────────────────────────────────────────────────────
builder.Services.AddSignalR();                 // SignalR
builder.Services.AddHostedService<EkgSimulator>();  // background fake-EKG publisher

var app = builder.Build();

// ─────────────────────────────────────────────────────────────
// 3.  HTTP pipeline
// ─────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ─────────────────────────────────────────────────────────────
// 4.  Endpoint mapping
// ─────────────────────────────────────────────────────────────
app.MapBlazorHub();                    // Blazor Server
app.MapHub<EkgHub>("/ekgHub");         // ← SignalR hub endpoint
app.MapFallbackToPage("/_Host");

app.Run();
