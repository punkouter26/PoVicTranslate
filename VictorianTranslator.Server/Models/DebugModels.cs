using System.Text.Json;

namespace VictorianTranslator.Server.Models;

public class DebugLogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Level { get; set; } = "Info"; // Info, Warning, Error, Critical
    public string? DataJson { get; set; }
    public string? ExceptionDetails { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    
    public T? GetData<T>() where T : class
    {
        if (string.IsNullOrEmpty(DataJson))
            return null;
            
        try
        {
            return JsonSerializer.Deserialize<T>(DataJson);
        }
        catch
        {
            return null;
        }
    }
    
    public void SetData<T>(T data) where T : class
    {
        if (data != null)
        {
            DataJson = JsonSerializer.Serialize(data, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
    }
}

public class DebugSummaryReport
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ReportPeriod { get; set; }
    public AppStateSnapshot AppState { get; set; } = new();
    public List<EventSummary> EventSummaries { get; set; } = new();
    public List<PerformanceMetric> PerformanceMetrics { get; set; } = new();
    public List<DebugLogEntry> CriticalEvents { get; set; } = new();
    public Dictionary<string, object> CustomMetrics { get; set; } = new();
}

public class AppStateSnapshot
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = string.Empty;
    public bool IsHealthy { get; set; } = true;
    public int ActiveConnections { get; set; }
    public Dictionary<string, string> ServiceStatus { get; set; } = new();
    public Dictionary<string, object> ConfigurationSnapshot { get; set; } = new();
    public SystemResourceUsage ResourceUsage { get; set; } = new();
}

public class EventSummary
{
    public string EventType { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime FirstOccurrence { get; set; }
    public DateTime LastOccurrence { get; set; }
    public string Level { get; set; } = string.Empty;
    public List<string> SampleMessages { get; set; } = new();
}

public class PerformanceMetric
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class SystemResourceUsage
{
    public double CpuUsagePercent { get; set; }
    public long MemoryUsageMB { get; set; }
    public long AvailableMemoryMB { get; set; }
    public double DiskUsagePercent { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
}
