using System.Diagnostics;
using System.Text.Json;
using VictorianTranslator.Server.Models;
using Microsoft.Extensions.Options;

namespace VictorianTranslator.Server.Services;

public class DebugLogService : IDebugLogService
{
    private readonly ILogger<DebugLogService> _logger;
    private readonly string _debugLogPath;
    private readonly string _reportsPath;
    private readonly string _sessionId;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    
    public DebugLogService(ILogger<DebugLogService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _sessionId = Guid.NewGuid().ToString("N")[..8];
        
        var debugRoot = Path.Combine(environment.ContentRootPath, "..", "DEBUG");
        _debugLogPath = Path.Combine(debugRoot, "logs");
        _reportsPath = Path.Combine(debugRoot, "reports");
        
        Directory.CreateDirectory(_debugLogPath);
        Directory.CreateDirectory(_reportsPath);
        
        _ = Task.Run(async () => await LogInitializationAsync("DebugLogService", "Debug logging service initialized", new { SessionId = _sessionId }));
    }

    public async Task LogInitializationAsync(string component, string message, object? data = null)
    {
        var entry = new DebugLogEntry
        {
            EventType = "Initialization",
            Component = component,
            Message = message,
            Level = "Info",
            SessionId = _sessionId
        };
        
        if (data != null)
            entry.SetData(data);
            
        await WriteLogEntryAsync(entry);
        _logger.LogInformation("[INIT] {Component}: {Message}", component, message);
    }

    public async Task LogEventAsync(string eventType, string message, object? data = null)
    {
        var entry = new DebugLogEntry
        {
            EventType = eventType,
            Component = "Application",
            Message = message,
            Level = "Info",
            SessionId = _sessionId
        };
        
        if (data != null)
            entry.SetData(data);
            
        await WriteLogEntryAsync(entry);
        _logger.LogInformation("[EVENT] {EventType}: {Message}", eventType, message);
    }

    public async Task LogInstabilityAsync(string component, string issue, object? diagnosticData = null)
    {
        var entry = new DebugLogEntry
        {
            EventType = "Instability",
            Component = component,
            Message = issue,
            Level = "Warning",
            SessionId = _sessionId
        };
        
        if (diagnosticData != null)
            entry.SetData(diagnosticData);
            
        await WriteLogEntryAsync(entry);
        _logger.LogWarning("[INSTABILITY] {Component}: {Issue}", component, issue);
    }

    public async Task LogStructuralFailureAsync(string component, string failure, Exception? exception = null, object? context = null)
    {
        var entry = new DebugLogEntry
        {
            EventType = "StructuralFailure",
            Component = component,
            Message = failure,
            Level = "Critical",
            SessionId = _sessionId,
            ExceptionDetails = exception?.ToString()
        };
        
        if (context != null)
            entry.SetData(context);
            
        await WriteLogEntryAsync(entry);
        _logger.LogCritical(exception, "[STRUCTURAL_FAILURE] {Component}: {Failure}", component, failure);
    }

    public async Task<DebugSummaryReport> GenerateSummaryReportAsync()
    {
        var endTime = DateTime.UtcNow;
        var startTime = endTime.AddHours(-1); // Last hour by default
        
        var logs = await GetLogsInTimeRangeAsync(startTime, endTime);
        
        var report = new DebugSummaryReport
        {
            GeneratedAt = endTime,
            ReportPeriod = endTime - startTime,
            AppState = await CaptureAppStateSnapshotAsync(),
            EventSummaries = GenerateEventSummaries(logs),
            PerformanceMetrics = await CollectPerformanceMetricsAsync(),
            CriticalEvents = logs.Where(l => l.Level == "Critical" || l.Level == "Error").ToList()
        };

        // Save report to file
        var reportFileName = $"summary_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{_sessionId}.json";
        var reportPath = Path.Combine(_reportsPath, reportFileName);
        
        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        }));
        
        _logger.LogInformation("Generated debug summary report: {ReportPath}", reportPath);
        
        return report;
    }

    public async Task<IEnumerable<DebugLogEntry>> GetRecentLogsAsync(int count = 100, string? eventType = null)
    {
        var allLogs = new List<DebugLogEntry>();
        
        var logFiles = Directory.GetFiles(_debugLogPath, "debug_*.json")
            .OrderByDescending(f => File.GetCreationTime(f))
            .Take(10); // Only check recent files
        
        foreach (var file in logFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(file);
                var entries = JsonSerializer.Deserialize<List<DebugLogEntry>>(content) ?? new List<DebugLogEntry>();
                allLogs.AddRange(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading debug log file: {File}", file);
            }
        }
        
        var query = allLogs.OrderByDescending(l => l.Timestamp).AsEnumerable();
        
        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(l => l.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase));
            
        return query.Take(count);
    }

    public async Task CleanupOldLogsAsync(TimeSpan retentionPeriod)
    {
        var cutoffTime = DateTime.UtcNow - retentionPeriod;
        
        var logFiles = Directory.GetFiles(_debugLogPath, "debug_*.json");
        var reportFiles = Directory.GetFiles(_reportsPath, "summary_report_*.json");
        
        foreach (var file in logFiles.Concat(reportFiles))
        {
            if (File.GetCreationTime(file) < cutoffTime)
            {
                try
                {
                    File.Delete(file);
                    _logger.LogInformation("Deleted old debug file: {File}", file);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting old debug file: {File}", file);
                }
            }
        }
    }

    private async Task WriteLogEntryAsync(DebugLogEntry entry)
    {
        await _fileLock.WaitAsync();
        try
        {
            var fileName = $"debug_{DateTime.UtcNow:yyyyMMdd}_{_sessionId}.json";
            var filePath = Path.Combine(_debugLogPath, fileName);
            
            List<DebugLogEntry> entries;
            
            if (File.Exists(filePath))
            {
                var content = await File.ReadAllTextAsync(filePath);
                entries = JsonSerializer.Deserialize<List<DebugLogEntry>>(content) ?? new List<DebugLogEntry>();
            }
            else
            {
                entries = new List<DebugLogEntry>();
            }
            
            entries.Add(entry);
            
            // Keep only recent entries in each file (max 1000)
            if (entries.Count > 1000)
            {
                entries = entries.OrderByDescending(e => e.Timestamp).Take(1000).ToList();
            }
            
            var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            await File.WriteAllTextAsync(filePath, json);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<DebugLogEntry>> GetLogsInTimeRangeAsync(DateTime startTime, DateTime endTime)
    {
        var allLogs = new List<DebugLogEntry>();
        
        var logFiles = Directory.GetFiles(_debugLogPath, "debug_*.json");
        
        foreach (var file in logFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(file);
                var entries = JsonSerializer.Deserialize<List<DebugLogEntry>>(content) ?? new List<DebugLogEntry>();
                allLogs.AddRange(entries.Where(e => e.Timestamp >= startTime && e.Timestamp <= endTime));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading debug log file: {File}", file);
            }
        }
        
        return allLogs.OrderBy(l => l.Timestamp).ToList();
    }

    private List<EventSummary> GenerateEventSummaries(List<DebugLogEntry> logs)
    {
        return logs
            .GroupBy(l => new { l.EventType, l.Component, l.Level })
            .Select(g => new EventSummary
            {
                EventType = g.Key.EventType,
                Component = g.Key.Component,
                Level = g.Key.Level,
                Count = g.Count(),
                FirstOccurrence = g.Min(l => l.Timestamp),
                LastOccurrence = g.Max(l => l.Timestamp),
                SampleMessages = g.Take(3).Select(l => l.Message).ToList()
            })
            .OrderByDescending(s => s.Count)
            .ToList();
    }

    private async Task<AppStateSnapshot> CaptureAppStateSnapshotAsync()
    {
        var process = Process.GetCurrentProcess();
        
        return new AppStateSnapshot
        {
            Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
            IsHealthy = true, // You can implement health checks here
            ServiceStatus = new Dictionary<string, string>
            {
                ["Translation"] = "Active",
                ["Speech"] = "Active",
                ["Lyrics"] = "Active"
            },
            ResourceUsage = new SystemResourceUsage
            {
                MemoryUsageMB = process.WorkingSet64 / 1024 / 1024,
                ThreadCount = process.Threads.Count,
                HandleCount = process.HandleCount
            }
        };
    }

    private async Task<List<PerformanceMetric>> CollectPerformanceMetricsAsync()
    {
        var process = Process.GetCurrentProcess();
        
        return new List<PerformanceMetric>
        {
            new() 
            { 
                Name = "Memory Usage", 
                Category = "System", 
                Value = process.WorkingSet64 / 1024.0 / 1024.0, 
                Unit = "MB" 
            },
            new() 
            { 
                Name = "Thread Count", 
                Category = "System", 
                Value = process.Threads.Count, 
                Unit = "count" 
            },
            new() 
            { 
                Name = "Handle Count", 
                Category = "System", 
                Value = process.HandleCount, 
                Unit = "count" 
            },
            new() 
            { 
                Name = "CPU Time", 
                Category = "System", 
                Value = process.TotalProcessorTime.TotalMilliseconds, 
                Unit = "ms" 
            }
        };
    }
}
