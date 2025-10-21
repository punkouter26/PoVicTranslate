using VictorianTranslator.Configuration;
using VictorianTranslator.Services;
using VictorianTranslator.Server.Services; // Added for ISpeechService and SpeechService
using VictorianTranslator.Server.Middleware; // Added for DebugLoggingMiddleware
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(builder.Environment.ContentRootPath, "..", "log.txt"),
        retainedFileCountLimit: 1,
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        fileSizeLimitBytes: null,
        rollOnFileSizeLimit: false,
        shared: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog(); // Use Serilog for logging

// Add services to the container.
builder.Services.AddApplicationInsightsTelemetry(); // Add Application Insights telemetry
builder.Services.AddControllers(); // Add controllers for Web API
builder.Services.AddEndpointsApiExplorer(); // Add API Explorer for Swagger/OpenAPI
builder.Services.AddSwaggerGen(); // Add Swagger generation

// Add Blazor WebAssembly hosting services
builder.Services.AddRazorPages();

// Configure API Settings
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

// Register Services
builder.Services.AddScoped<ITranslationService, TranslationService>();
builder.Services.AddScoped<ILyricsService, LyricsService>();
builder.Services.AddScoped<IAudioSynthesisService, AudioSynthesisService>();
builder.Services.AddScoped<IConfigurationValidator, ConfigurationValidator>();
builder.Services.AddScoped<IDiagnosticService, DiagnosticService>();
builder.Services.AddScoped<ILyricsManagementService, LyricsManagementService>();
builder.Services.AddSingleton<IDebugLogService, DebugLogService>(); // Add Debug Log Service as singleton

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // For production, allow the same origin
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .SetIsOriginAllowed(origin => true)
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseCors(); // Always use CORS
app.UseMiddleware<DebugLoggingMiddleware>(); // Add debug logging middleware
app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Enable Swagger in all environments per requirements
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PoVicTranslate API v1");
    if (!app.Environment.IsDevelopment())
    {
        // In production, Swagger is available but not as the default page
        c.RoutePrefix = "swagger";
    }
});

// Configure endpoints - ORDER MATTERS!
app.MapControllers(); // Handle API routes first
app.MapRazorPages();

// Final fallback for the Blazor app - this should be last
// MapFallbackToFile automatically handles non-API routes
app.MapFallbackToFile("index.html");

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
