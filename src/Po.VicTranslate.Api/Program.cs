using Po.VicTranslate.Api.Configuration;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Middleware;
using Po.VicTranslate.Api.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using Serilog;
using Serilog.Events;
using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for HTTP-only mode if requested (for E2E tests)
// Only override Kestrel if explicitly requested via environment variable
var disableHttpsRedirection = Environment.GetEnvironmentVariable("DISABLE_HTTPS_REDIRECTION");
var httpOnlyMode = !string.IsNullOrEmpty(disableHttpsRedirection) && 
                   (disableHttpsRedirection.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                    disableHttpsRedirection == "1");

Console.WriteLine($"DEBUG: DISABLE_HTTPS_REDIRECTION={disableHttpsRedirection}, httpOnlyMode={httpOnlyMode}");
                    
if (httpOnlyMode)
{
    var port = builder.Configuration.GetValue<int>("HTTP_PORT", 5002); // Default to 5002 for E2E tests
    Console.WriteLine($"DEBUG: Configuring Kestrel for HTTP-only mode on port {port}");
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenLocalhost(port); // HTTP only
    });
}
else
{
    Console.WriteLine("DEBUG: Using default Kestrel configuration from launchSettings.json");
}

// Phase 4: Configure Serilog with structured logging and Application Insights sink
var connectionString = builder.Configuration["ApplicationInsights:ConnectionString"] 
    ?? builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
var telemetryConfig = TelemetryConfiguration.CreateDefault();
if (!string.IsNullOrEmpty(connectionString))
{
    telemetryConfig.ConnectionString = connectionString;
}

var logConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "PoVicTranslate.Api")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

// Only add file logging in development
if (builder.Environment.IsDevelopment())
{
    logConfig.WriteTo.File(
        Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "log.txt"),
        retainedFileCountLimit: 1,
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        fileSizeLimitBytes: null,
        rollOnFileSizeLimit: false,
        shared: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
    );
}

// Add Application Insights sink
logConfig.WriteTo.ApplicationInsights(
    telemetryConfig,
    TelemetryConverter.Traces,
    LogEventLevel.Information);

Log.Logger = logConfig.CreateLogger();

builder.Host.UseSerilog(); // Use Serilog for logging

Log.Information("PoVicTranslate API starting up with structured logging and Application Insights telemetry");

try
{
    Console.WriteLine("DEBUG: Adding services to container...");

// Add services to the container.
builder.Services.AddApplicationInsightsTelemetry(); // Add Application Insights telemetry
Console.WriteLine("DEBUG: Application Insights telemetry added");

// REQUIRED: Add RFC 7807 Problem Details exception handler
builder.Services.AddExceptionHandler<Po.VicTranslate.Api.Middleware.ProblemDetailsExceptionHandler>();
builder.Services.AddProblemDetails();

// REQUIRED: Add Health Checks with readiness and liveness semantics
builder.Services.AddHealthChecks()
    .AddCheck<AzureOpenAIHealthCheck>("AzureOpenAI", tags: new[] { "ready", "external" })
    .AddCheck<AzureSpeechHealthCheck>("AzureSpeech", tags: new[] { "ready", "external" })
    .AddCheck<InternetConnectivityHealthCheck>("InternetConnectivity", tags: new[] { "external" });

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
builder.Services.AddSingleton<ICustomTelemetryService, CustomTelemetryService>(); // Phase 4: Custom telemetry

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

// REQUIRED: Configure exception handling middleware (must be early in pipeline)
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}

app.UseCors(); // Always use CORS
app.UseMiddleware<DebugLoggingMiddleware>(); // Add debug logging middleware

// Only use HTTPS redirection if not explicitly disabled (for E2E tests)
if (!httpOnlyMode)
{
    app.UseHttpsRedirection();
}

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

// REQUIRED: Map Health Check endpoint at /api/health (readiness and liveness)
app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Checks = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description ?? string.Empty,
                Error = e.Value.Exception?.Message
            })
        });
        await context.Response.WriteAsync(result);
    }
});

// Liveness endpoint (simple check if app is running)
app.MapHealthChecks("/api/health/live", new HealthCheckOptions
{
    Predicate = _ => false, // Exclude all checks, just return if app is alive
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            Status = "Alive",
            Timestamp = DateTime.UtcNow
        });
        await context.Response.WriteAsync(result);
    }
});

// Readiness endpoint (checks critical dependencies)
app.MapHealthChecks("/api/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            CriticalChecks = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description ?? string.Empty
            })
        });
        await context.Response.WriteAsync(result);
    }
});

// Configure endpoints - ORDER MATTERS!
app.MapControllers(); // Handle API routes first
app.MapRazorPages();

// Final fallback for the Blazor app - this should be last
// MapFallbackToFile automatically handles non-API routes
app.MapFallbackToFile("index.html");

    Console.WriteLine("DEBUG: About to call app.Run()...");
    app.Run();
    Console.WriteLine("DEBUG: app.Run() completed (this should never print)");
}
catch (Exception ex)
{
    Console.WriteLine($"FATAL ERROR during startup: {ex.GetType().Name}: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Make Program accessible to integration tests
public partial class Program { }
