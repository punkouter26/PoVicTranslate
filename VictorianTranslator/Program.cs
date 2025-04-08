using Microsoft.AspNetCore.Components;
using VictorianTranslator.Components;
using VictorianTranslator.Configuration;
using VictorianTranslator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure API Settings
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

// Register Services (HttpClient removed as it's no longer directly needed by registered services)
// Note: If any other service added later needs HttpClientFactory, re-add builder.Services.AddHttpClient();
builder.Services.AddScoped<ITranslationService, TranslationService>();
builder.Services.AddScoped<ITextToSpeechService, TextToSpeechService>();
builder.Services.AddScoped<ILyricsService, LyricsService>();
builder.Services.AddScoped<IDiagnosticService, DiagnosticService>(); // Register the diagnostic service

// Add CORS policy for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
