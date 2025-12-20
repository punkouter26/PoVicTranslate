using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Po.VicTranslate.Client;
using Po.VicTranslate.Client.Services;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiBaseUrl = config["ApiBaseUrl"];

    // Use the configured ApiBaseUrl if it exists and is not empty, 
    // otherwise fall back to the host environment base address (same-origin)
    var baseAddress = !string.IsNullOrEmpty(apiBaseUrl) ? apiBaseUrl : builder.HostEnvironment.BaseAddress;

    return new HttpClient { BaseAddress = new Uri(baseAddress) };
});
builder.Services.AddScoped<ClientTranslationService>();
builder.Services.AddScoped<ClientLyricsService>();
builder.Services.AddScoped<ITranslationOrchestrator, TranslationOrchestrator>();

// UI Services
builder.Services.AddScoped<HistoryService>();

// Radzen Services
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

await builder.Build().RunAsync();
