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

if (httpOnlyMode)
{
    var port = builder.Configuration.GetValue<int>("HTTP_PORT", 5002); // Default to 5002 for E2E tests
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenLocalhost(port); // HTTP only
    });
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
    .MinimumLevel.Override("Microsoft.ApplicationInsights", LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

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

Log.Information("PoVicTranslate API starting up");

try
{
    // Add services to the container.
    // Disable verbose Application Insights console output in development
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.EnableDebugLogger = false;
        options.EnableDiagnosticsTelemetryModule = !builder.Environment.IsDevelopment();
    });

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

    // Phase 8: Performance Optimization - Memory Caching
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<Po.VicTranslate.Api.Services.Caching.ICacheService, Po.VicTranslate.Api.Services.Caching.CacheService>();

    // Add HttpClient factory for REST API calls (Speech Service)
    builder.Services.AddHttpClient();

    // Configure API Settings
    builder.Services.Configure<ApiSettings>(
        builder.Configuration.GetSection("ApiSettings"));

    // Register Services
    builder.Services.AddScoped<ITranslationService, TranslationService>();
    builder.Services.AddScoped<ILyricsService, LyricsService>();

    // Phase 6: SOLID Refactoring - Speech Config Validator (SRP)
    builder.Services.AddSingleton<Po.VicTranslate.Api.Services.Validation.ISpeechConfigValidator, Po.VicTranslate.Api.Services.Validation.SpeechConfigValidator>();
    builder.Services.AddScoped<IAudioSynthesisService, AudioSynthesisService>();

    // Phase 9: Security - Input Validation and Sanitization
    builder.Services.AddSingleton<Po.VicTranslate.Api.Services.Validation.IInputValidator, Po.VicTranslate.Api.Services.Validation.InputValidator>();

    // Phase 6: SOLID Refactoring - Diagnostic Validators (SRP)
    builder.Services.AddScoped<Po.VicTranslate.Api.Services.Validation.IDiagnosticValidator, Po.VicTranslate.Api.Services.Validation.AzureOpenAIDiagnosticValidator>();
    builder.Services.AddScoped<Po.VicTranslate.Api.Services.Validation.IDiagnosticValidator, Po.VicTranslate.Api.Services.Validation.AzureSpeechDiagnosticValidator>();
    builder.Services.AddScoped<Po.VicTranslate.Api.Services.Validation.IDiagnosticValidator, Po.VicTranslate.Api.Services.Validation.InternetConnectivityDiagnosticValidator>();
    builder.Services.AddScoped<IConfigurationValidator, ConfigurationValidator>();

    builder.Services.AddScoped<IDiagnosticService, DiagnosticService>();

    // Phase 7: DRY Refactoring - Lyrics Utility Service
    builder.Services.AddSingleton<Po.VicTranslate.Api.Services.Lyrics.ILyricsUtilityService, Po.VicTranslate.Api.Services.Lyrics.LyricsUtilityService>();

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
    app.UseMiddleware<ApiResponseTimeMiddleware>(); // Track API response times for Application Insights

    // Only use HTTPS redirection when it can be resolved (avoids noisy warnings in HTTP-only dev profiles)
    if (!httpOnlyMode)
    {
        var httpsPort = builder.Configuration.GetValue<int?>("ASPNETCORE_HTTPS_PORT");
        var urls = builder.Configuration["ASPNETCORE_URLS"] ?? string.Empty;
        var hasHttpsUrl = urls.Contains("https://", StringComparison.OrdinalIgnoreCase);

        if (httpsPort.HasValue || hasHttpsUrl)
        {
            app.UseHttpsRedirection();
        }
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

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Make Program accessible to integration tests
public partial class Program { }
