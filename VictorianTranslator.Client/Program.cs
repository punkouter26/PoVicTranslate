using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VictorianTranslator.Client;
using VictorianTranslator.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ClientTranslationService>();
builder.Services.AddScoped<ClientLyricsService>();
builder.Services.AddScoped<ClientSpeechService>();
builder.Services.AddScoped<ITranslationOrchestrator, TranslationOrchestrator>();

await builder.Build().RunAsync();
