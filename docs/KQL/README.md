# KQL Queries - PoVicTranslate Application Insights

This folder contains Kusto Query Language (KQL) queries for monitoring and analyzing the PoVicTranslate application through Azure Application Insights.

## Phase 4: Debugging & Telemetry

### Overview

The application implements comprehensive telemetry using:
- **Serilog** for structured logging with Application Insights sink
- **Custom Telemetry Service** for high-value business metrics
- **Application Insights** for centralized monitoring and analytics

## Essential Queries (Required)

### 1. User Activity (`01-UserActivity.kql`)
**Purpose:** Track user engagement and session patterns

**Key Metrics:**
- Daily Active Users (DAU) trend
- Unique sessions per hour
- Most popular activities
- Activity heatmap by day/hour
- User retention analysis

**Use Cases:**
- Monitor user adoption and engagement
- Identify peak usage times
- Track feature popularity
- Measure retention rates

### 2. Slow Requests (`02-SlowRequests.kql`)
**Purpose:** Identify performance bottlenecks and slow API endpoints

**Key Metrics:**
- Top 10 slowest requests
- Average response time by endpoint (P50, P95, P99)
- Performance degradation detection
- Dependency call analysis
- Custom performance metrics

**Use Cases:**
- Performance optimization
- SLA monitoring
- Capacity planning
- Identify regression issues

### 3. Error Rate (`03-ErrorRate.kql`)
**Purpose:** Monitor application health and failure patterns

**Key Metrics:**
- Overall error rate percentage
- Error rate by endpoint
- Error distribution by HTTP status code
- Exception correlation
- Availability percentage
- Error spike detection

**Use Cases:**
- Health monitoring and alerts
- Incident response and investigation
- Track SLA compliance (99.9% uptime)
- Identify client vs server issues

## Custom Telemetry Queries (`04-CustomTelemetry.kql`)

### Translation Analytics
- Translation volume and success rate
- Performance by text length category
- Failed translation analysis

### Lyrics Analytics
- Most popular songs and artists
- Access patterns (View, Create, Update, Delete)
- Content engagement metrics

### Audio Synthesis Analytics
- Synthesis performance and output size
- Efficiency metrics (bytes per ms)
- Success rates and failures

### Data Usage Analytics
- Operations by entity type (Read, Create, Update, Delete)
- Data access trends over time
- Record volume tracking

### Performance Metrics
- Custom operation performance tracking
- Slow operation detection
- Success rate monitoring

### Cross-Feature Analytics
- Feature usage distribution
- End-to-end workflow analysis (translation + audio)
- Conversion rate tracking

## How to Use

### In Azure Portal

1. Navigate to **Azure Portal** > **Application Insights** resource
2. Click **Logs** in the left menu
3. Copy query from `.kql` file
4. Paste into query editor
5. Click **Run**
6. View results, export to Excel/CSV, or pin to dashboard

### Query Customization

**Time Ranges:**
```kql
| where timestamp > ago(7d)    // Last 7 days
| where timestamp > ago(24h)   // Last 24 hours
| where timestamp > ago(1h)    // Last hour
| where timestamp > ago(30d)   // Last 30 days
```

**Filters:**
```kql
| where name == "TranslationRequest"
| where success == false
| where duration > 5000
| where customDimensions.Success == "true"
```

**Aggregations:**
```kql
| summarize count() by bin(timestamp, 1h)  // Hourly
| summarize avg(duration), max(duration)   // Statistics
| summarize percentile(duration, 95)       // P95
```

### Creating Alerts

1. Run query in Application Insights > Logs
2. Click **+ New alert rule**
3. Define condition (e.g., error rate > 5%)
4. Set action group for notifications
5. Save alert rule

### Creating Dashboards

1. Run query and verify results
2. Click **Pin to dashboard**
3. Select existing dashboard or create new
4. Add multiple query tiles for comprehensive monitoring

## Custom Telemetry Events

The application tracks these custom events via `CustomTelemetryService`:

| Event Name | Purpose | Key Dimensions | Key Measurements |
|:---|:---|:---|:---|
| `TranslationRequest` | Translation operations | InputLanguage, Success, TextLengthCategory | TextLength, DurationMs |
| `LyricsAccess` | Lyrics CRUD operations | SongTitle, Artist, AccessType | - |
| `AudioSynthesis` | Audio generation | Success, TextLengthCategory | TextLength, DurationMs, AudioSizeBytes |
| `DataUsage` | Data operations | Operation, EntityType | RecordCount |
| `PerformanceMetric` | Custom perf tracking | OperationName, Success | DurationMs |
| `UserActivity` | User engagement | Activity, UserId | - |

## Monitoring Recommendations

### Critical Alerts

1. **Error Rate > 5%** (last 5 minutes)
   - Action: Page on-call engineer
   
2. **P95 Response Time > 5 seconds** (last 10 minutes)
   - Action: Send notification to team

3. **Availability < 99%** (last 15 minutes)
   - Action: Escalate to incident response

### Daily Reviews

- User activity trends
- Top 10 slowest requests
- Error summary and patterns
- Feature usage distribution

### Weekly Reviews

- Performance degradation analysis
- Most popular content (songs/artists)
- Audio synthesis usage and cost
- Data growth trends

## Performance Targets

| Metric | Target | Alert Threshold |
|:---|:---|:---|
| Error Rate | < 1% | > 5% |
| P95 Response Time | < 2000ms | > 5000ms |
| Availability | > 99.9% | < 99% |
| Translation Success Rate | > 95% | < 90% |
| Audio Synthesis Success Rate | > 98% | < 95% |

## Resources

- [KQL Reference](https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/)
- [Application Insights Overview](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Kusto Query Best Practices](https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/best-practices)
- [Application Insights API](https://learn.microsoft.com/en-us/rest/api/application-insights/)

## Phase 4 Implementation Details

### Structured Logging (Serilog)
- Configured with Application Insights sink
- Enriched with Application name and Environment
- Log levels: Information (minimum), Warning (Microsoft/System)
- Multiple outputs: Console, Debug, File, Application Insights

### Custom Telemetry Service
- Singleton service injected via DI
- Tracks business-critical events
- Includes structured properties and measurements
- Integrates with Serilog for correlated logging

### Integration Points
- TranslationController: Tracks translation requests with timing
- LyricsController: Tracks content access patterns (TODO)
- AudioSynthesisController: Tracks audio generation metrics (TODO)
- Middleware: Automatic request/response tracking via Application Insights

---

*Last Updated: Phase 4 - October 29, 2025*  
*For questions or improvements, contact the development team*
