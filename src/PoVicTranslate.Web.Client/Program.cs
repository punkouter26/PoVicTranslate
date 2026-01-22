using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoVicTranslate.Web.Client.Services;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure HttpClient to use the host server
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register services
builder.Services.AddScoped<ClientTranslationService>();
builder.Services.AddScoped<ClientLyricsService>();
builder.Services.AddScoped<ClientSpeechService>();
builder.Services.AddScoped<ITranslationOrchestrator, TranslationOrchestrator>();
builder.Services.AddScoped<HistoryService>();

// Radzen services
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

await builder.Build().RunAsync();
