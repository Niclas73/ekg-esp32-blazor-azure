using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using BlazorApp.Data;
using BlazorApp.Hubs;
using BlazorApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────
// 1.  Framework-tjänster
// ─────────────────────────────────────────────
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// ─────────────────────────────────────────────
// 2.  Realtid + EKG-simulator
// ─────────────────────────────────────────────
builder.Services.AddSignalR();                     // SignalR-nav
builder.Services.AddHostedService<EkgSimulator>(); // vår nya Gauss-baserade EKG-generator

var app = builder.Build();

// ─────────────────────────────────────────────
// 3.  HTTP-pipeline
// ─────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ─────────────────────────────────────────────
// 4.  Endpoints
// ─────────────────────────────────────────────
app.MapBlazorHub();              // Blazor Server
app.MapHub<EkgHub>("/ekgHub");   // SignalR-hub
app.MapFallbackToPage("/_Host");

app.Run();
