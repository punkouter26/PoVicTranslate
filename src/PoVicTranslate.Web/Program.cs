using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PoVicTranslate.Web.Components;
using PoVicTranslate.Web.Configuration;
using PoVicTranslate.Web.Endpoints;
using PoVicTranslate.Web.HealthChecks;
using PoVicTranslate.Web.Services;
using PoVicTranslate.Web.Services.Caching;
using PoVicTranslate.Web.Services.Lyrics;
using PoVicTranslate.Web.Services.Validation;
using Radzen;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire Service Defaults (OpenTelemetry, Health Checks, Resilience)
builder.AddServiceDefaults();

// Configure Azure Key Vault using DefaultAzureCredential
// Map Key Vault secret names (with dashes) to configuration keys (with colons)
var keyVaultName = builder.Configuration["KeyVault:Name"] ?? "kv-poshared";
var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
var credential = new DefaultAzureCredential();

builder.Configuration.AddAzureKeyVault(
    keyVaultUri,
    credential,
    new KeyVaultSecretManager());

// Map Key Vault secrets to ApiSettings configuration
// Key Vault uses dashes, configuration uses colons
builder.Services.PostConfigure<ApiSettings>(settings =>
{
    var config = builder.Configuration;
    settings.AzureOpenAIApiKey = config["AzureOpenAI-ApiKey"] ?? settings.AzureOpenAIApiKey;
    settings.AzureOpenAIEndpoint = config["AzureOpenAI-Endpoint"] ?? settings.AzureOpenAIEndpoint;
    settings.AzureOpenAIDeploymentName = config["AzureOpenAI-DeploymentName"] ?? settings.AzureOpenAIDeploymentName;
    settings.AzureSpeechSubscriptionKey = config["AzureSpeech-SubscriptionKey"] ?? settings.AzureSpeechSubscriptionKey;
    settings.AzureSpeechRegion = config["AzureSpeech-Region"] ?? settings.AzureSpeechRegion;
});

// Configure Serilog
var connectionString = builder.Configuration["ApplicationInsights-ConnectionString"]
    ?? builder.Configuration["ApplicationInsights:ConnectionString"]
    ?? builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
var telemetryConfig = TelemetryConfiguration.CreateDefault();
if (!string.IsNullOrEmpty(connectionString))
{
    telemetryConfig.ConnectionString = connectionString;
}

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "PoVicTranslate.Web")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(telemetryConfig, TelemetryConverter.Traces, LogEventLevel.Information)
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("PoVicTranslate Web starting up");

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.EnableDebugLogger = false;
    options.EnableDiagnosticsTelemetryModule = !builder.Environment.IsDevelopment();
});

// Add health checks with external dependency checks
// InternetConnectivity check is non-critical (Degraded on failure, not Unhealthy)
builder.Services.AddHealthChecks()
    .AddCheck<AzureOpenAIHealthCheck>("AzureOpenAI", tags: ["ready", "external"])
    .AddCheck<AzureSpeechHealthCheck>("AzureSpeech", tags: ["ready", "external"])
    .AddCheck<InternetConnectivityHealthCheck>("InternetConnectivity", 
        failureStatus: HealthStatus.Degraded, tags: ["external"]);

// Add Blazor components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add Radzen services (DialogService, NotificationService, TooltipService)
builder.Services.AddRadzenComponents();

// Add memory caching
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, CacheService>();

// Add HttpClient factory for general use
builder.Services.AddHttpClient();

// Register scoped HttpClient for client services during SSR prerendering
// During SSR, client services need HttpClient with base address pointing to self
builder.Services.AddScoped(sp =>
{
    var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
    var request = httpContextAccessor?.HttpContext?.Request;
    var baseAddress = request != null
        ? new Uri($"{request.Scheme}://{request.Host}")
        : new Uri("http://localhost:5002");
    return new HttpClient { BaseAddress = baseAddress };
});
builder.Services.AddHttpContextAccessor();

// Configure API Settings
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// Register core services
builder.Services.AddScoped<ITranslationService, TranslationService>();
builder.Services.AddScoped<ILyricsService, LyricsService>();

// Register validation services
builder.Services.AddSingleton<ISpeechConfigValidator, SpeechConfigValidator>();
builder.Services.AddScoped<IAudioSynthesisService, AudioSynthesisService>();
builder.Services.AddSingleton<IInputValidator, InputValidator>();

// Register diagnostic validators
builder.Services.AddScoped<IDiagnosticValidator, AzureOpenAIDiagnosticValidator>();
builder.Services.AddScoped<IDiagnosticValidator, AzureSpeechDiagnosticValidator>();
builder.Services.AddScoped<IDiagnosticValidator, InternetConnectivityDiagnosticValidator>();
builder.Services.AddScoped<IConfigurationValidator, ConfigurationValidator>();
builder.Services.AddScoped<IDiagnosticService, DiagnosticService>();

// Register utility services
builder.Services.AddSingleton<ILyricsUtilityService, LyricsUtilityService>();
builder.Services.AddSingleton<ICustomTelemetryService, CustomTelemetryService>();

// Register client-side services for InteractiveAuto SSR prerendering
builder.Services.AddScoped<PoVicTranslate.Web.Client.Services.ClientTranslationService>();
builder.Services.AddScoped<PoVicTranslate.Web.Client.Services.ClientLyricsService>();
builder.Services.AddScoped<PoVicTranslate.Web.Client.Services.ClientSpeechService>();
builder.Services.AddScoped<PoVicTranslate.Web.Client.Services.HistoryService>();
builder.Services.AddScoped<PoVicTranslate.Web.Client.Services.ITranslationOrchestrator, 
    PoVicTranslate.Web.Client.Services.TranslationOrchestrator>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyHeader().AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)
                .AllowCredentials();
        }
    });
});

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapStaticAssets();
app.UseAntiforgery();

// Map Aspire default endpoints (health checks)
app.MapDefaultEndpoints();

// Map minimal API endpoints
app.MapTranslationEndpoints();
app.MapLyricsEndpoints();
app.MapSpeechEndpoints();

// Map OpenAPI endpoints
app.MapOpenApi();

// Map Blazor components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PoVicTranslate.Web.Client._Imports).Assembly);

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
