# Product Requirements Document (PRD)
# PoVicTranslate - Victorian English Translation System

**Version:** 1.0  
**Date:** October 29, 2025  
**Status:** Production Ready  
**Author:** Development Team  

---

## Executive Summary

PoVicTranslate is a full-stack web application that provides AI-powered translation from modern English to Victorian-era English, comprehensive lyrics management with Cockney rhyming slang support, and text-to-speech synthesis. The application leverages Azure OpenAI GPT-4 for natural language processing and Azure Cognitive Services for audio generation.

### Problem Statement

Historical language understanding and recreation requires:
- Accurate translation to period-appropriate dialects
- Management of historical lyrics and phrases
- Audio synthesis for educational and entertainment purposes
- Reliable cloud-based infrastructure

### Solution

A cloud-native application built on Azure App Service that:
- Translates modern English to Victorian English using GPT-4
- Manages song lyrics with CRUD operations
- Synthesizes speech audio from text
- Provides comprehensive monitoring and health checks
- Delivers a responsive web-based user interface

---

## Product Goals

### Primary Objectives

1. **Translation Accuracy**: Provide historically accurate Victorian English translations
2. **User Experience**: Deliver intuitive, responsive UI for all features
3. **Reliability**: Maintain 99.9% uptime with comprehensive health monitoring
4. **Performance**: Respond to translation requests within 3 seconds (P95)
5. **Observability**: Track all user actions and system metrics

### Success Metrics

- **Translation Success Rate**: >95% successful translations
- **API Availability**: >99.9% uptime
- **Response Time**: <3s for P95 translation requests
- **Error Rate**: <1% of total requests
- **Test Coverage**: >80% line coverage (target)

---

## Target Audience

### Primary Users

1. **Educators**: Teaching historical English and literature
2. **Writers**: Researching period-appropriate dialogue
3. **Historians**: Studying Victorian-era language patterns
4. **Entertainment**: Creating historically accurate content

### Secondary Users

1. **Linguists**: Analyzing language evolution
2. **Students**: Learning about historical English
3. **Hobbyists**: Exploring Victorian culture

---

## Functional Requirements

### FR-1: Translation Service

**Description**: Translate modern English text to Victorian-era English

**Requirements**:
- **FR-1.1**: Accept text input (min 1 char, max 2000 chars)
- **FR-1.2**: Call Azure OpenAI GPT-4 with Victorian English prompt
- **FR-1.3**: Return translated text with metadata (timestamp, confidence)
- **FR-1.4**: Handle translation errors gracefully
- **FR-1.5**: Log all translation attempts with telemetry
- **FR-1.6**: Support batch translation (future)

**API Endpoint**: `POST /api/translation/translate`

**Request**:
```json
{
  "text": "Hello, how are you today?",
  "options": {
    "formalityLevel": "polite",
    "includeSlang": false
  }
}
```

**Response**:
```json
{
  "originalText": "Hello, how are you today?",
  "translatedText": "Good day to you, kind sir. How do you fare this fine day?",
  "timestamp": "2025-10-29T12:00:00Z",
  "processingTimeMs": 1250
}
```

**Success Criteria**:
- Translation completes within 3 seconds (P95)
- Output is grammatically correct Victorian English
- Handles edge cases (empty text, special characters)

---

### FR-2: Lyrics Management

**Description**: Full CRUD operations for song lyrics database

**Requirements**:
- **FR-2.1**: Create new lyrics entry with title, artist, lyrics text, Cockney flag
- **FR-2.2**: Read all lyrics with pagination support
- **FR-2.3**: Read single lyrics entry by ID
- **FR-2.4**: Update existing lyrics entry
- **FR-2.5**: Delete lyrics entry by ID
- **FR-2.6**: Search lyrics by title or artist (future)
- **FR-2.7**: Filter lyrics by Cockney flag

**API Endpoints**:
- `GET /api/lyrics` - List all lyrics
- `GET /api/lyrics/{id}` - Get single entry
- `POST /api/lyrics` - Create new entry
- `PUT /api/lyrics/{id}` - Update entry
- `DELETE /api/lyrics/{id}` - Delete entry

**Data Model**:
```json
{
  "id": 1,
  "title": "My Old Man Said Follow the Van",
  "artist": "Traditional",
  "lyrics": "My old man said follow the van...",
  "isCockney": true,
  "createdDate": "2025-10-29T12:00:00Z",
  "lastModified": "2025-10-29T12:00:00Z"
}
```

**Success Criteria**:
- CRUD operations complete within 500ms
- Data persists across application restarts
- Validation prevents invalid data entry

---

### FR-3: Audio Synthesis

**Description**: Convert text to speech audio using Azure Cognitive Services

**Requirements**:
- **FR-3.1**: Accept text input (min 1 char, max 5000 chars)
- **FR-3.2**: Call Azure Speech Service with specified voice
- **FR-3.3**: Return audio file (WAV format)
- **FR-3.4**: Support multiple voice options (British English)
- **FR-3.5**: Handle synthesis errors gracefully
- **FR-3.6**: Cache frequently requested audio (future)

**API Endpoint**: `POST /api/audio/synthesize`

**Request**:
```json
{
  "text": "Good day to you, kind sir.",
  "voice": "en-GB-RyanNeural",
  "speed": 1.0,
  "pitch": 0
}
```

**Response**: Binary audio file (audio/wav)

**Success Criteria**:
- Synthesis completes within 5 seconds
- Audio quality is clear and natural
- Supports British English voices

---

### FR-4: Health Monitoring

**Description**: Comprehensive health checks for all system dependencies

**Requirements**:
- **FR-4.1**: Liveness endpoint - confirms application is running
- **FR-4.2**: Readiness endpoint - confirms dependencies are available
- **FR-4.3**: Full health endpoint - detailed status of all checks
- **FR-4.4**: Check Azure OpenAI connectivity
- **FR-4.5**: Check Azure Speech Service connectivity
- **FR-4.6**: Check internet connectivity
- **FR-4.7**: Return health status in JSON format

**API Endpoints**:
- `GET /api/health` - Full health check
- `GET /api/health/live` - Liveness probe
- `GET /api/health/ready` - Readiness probe

**Response**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-10-29T12:00:00Z",
  "checks": [
    {
      "name": "AzureOpenAI",
      "status": "Healthy",
      "description": "Azure OpenAI Service is reachable"
    },
    {
      "name": "AzureSpeech",
      "status": "Healthy",
      "description": "Azure Speech Service is reachable"
    },
    {
      "name": "InternetConnectivity",
      "status": "Healthy",
      "description": "Internet connection is available"
    }
  ]
}
```

**Success Criteria**:
- Health checks respond within 1 second
- Accurate status reporting for all dependencies
- Integrates with Azure Monitor for alerting

---

## Non-Functional Requirements

### NFR-1: Performance

- **Response Time**:
  - Translation API: P50 < 1.5s, P95 < 3s, P99 < 5s
  - Lyrics API: P50 < 200ms, P95 < 500ms
  - Health Checks: < 1s
- **Throughput**: Support 100 concurrent users
- **Scalability**: Horizontal scaling via Azure App Service

### NFR-2: Reliability

- **Availability**: 99.9% uptime SLA
- **Error Handling**: All errors logged with detailed context
- **Retry Logic**: Automatic retry for transient failures (3 attempts)
- **Circuit Breaker**: Prevent cascading failures to external services

### NFR-3: Security

- **Authentication**: Azure AD integration (future)
- **API Keys**: Secure storage in Azure Key Vault or App Service Configuration
- **HTTPS**: All traffic encrypted with TLS 1.2+
- **CORS**: Configurable origin policies
- **Input Validation**: Prevent injection attacks

### NFR-4: Observability

- **Structured Logging**: Serilog with Application Insights sink
- **Custom Telemetry**: Track 6 event types (see Telemetry section)
- **Distributed Tracing**: Correlation IDs for request tracking
- **Metrics**: Response times, error rates, dependency status
- **KQL Queries**: Pre-built queries for common scenarios

### NFR-5: Maintainability

- **Code Coverage**: >80% target (currently 33.7%)
- **Documentation**: Inline XML comments, README, PRD
- **Code Style**: .editorconfig and dotnet format
- **Versioning**: Semantic versioning (SemVer)
- **CI/CD**: Automated testing and deployment

### NFR-6: Usability

- **Responsive Design**: Mobile, tablet, desktop support
- **Accessibility**: WCAG 2.1 AA compliance (target)
- **Loading States**: Visual feedback during async operations
- **Error Messages**: User-friendly, actionable error text
- **API Documentation**: Swagger/OpenAPI spec

---

## Technical Architecture

### System Components

```
┌─────────────────────────────────────────────────────────────┐
│                     Internet Users                          │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              Azure App Service (PoVicTranslate)             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           Po.VicTranslate.Api (ASP.NET Core)         │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  Controllers │ Services │ Middleware │ Health  │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  │           Po.VicTranslate.Client (Blazor WASM)       │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  Pages │ Components │ ViewModels │ Services    │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────┬──────────────────┬──────────────────────┘
                   │                  │
        ┌──────────┴────────┐  ┌─────┴──────────┐
        ▼                   ▼  ▼                ▼
┌────────────────┐  ┌────────────────┐  ┌────────────────┐
│ Azure OpenAI   │  │ Azure Speech   │  │  Application   │
│   (GPT-4)      │  │    Service     │  │    Insights    │
└────────────────┘  └────────────────┘  └────────────────┘
```

### Technology Stack

**Backend:**
- ASP.NET Core 9.0 Web API
- Serilog 9.0.0 (logging)
- Azure SDK for .NET (OpenAI, Speech, App Insights)

**Frontend:**
- Blazor WebAssembly .NET 9.0
- HTML5, CSS3, JavaScript

**Cloud Services:**
- Azure App Service (Windows, .NET 9.0)
- Azure OpenAI (GPT-4 deployment)
- Azure Cognitive Services (Speech)
- Azure Application Insights
- Azure Log Analytics Workspace

**DevOps:**
- GitHub (source control)
- GitHub Actions (CI/CD)
- Azure CLI (infrastructure management)
- Bicep (infrastructure as code)

**Testing:**
- xUnit 3.1.0 (unit testing)
- Moq 4.20.72 (mocking)
- FluentAssertions 8.8.0 (assertions)
- Playwright (E2E testing)
- Coverlet (code coverage)

---

## Data Model

### Lyrics Entity

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | int | Yes | Primary key (auto-increment) |
| Title | string | Yes | Song title (max 200 chars) |
| Artist | string | Yes | Artist name (max 100 chars) |
| Lyrics | string | Yes | Full lyrics text (max 10,000 chars) |
| IsCockney | bool | Yes | Cockney rhyming slang flag |
| CreatedDate | DateTime | Yes | Timestamp of creation |
| LastModified | DateTime | Yes | Timestamp of last update |

**Storage**: In-memory list (development), Azure Table Storage (future)

**Indexes**: Id (primary), Title (future), Artist (future)

---

## API Specifications

### Translation API

**POST** `/api/translation/translate`

**Request Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "text": "string (1-2000 chars, required)"
}
```

**Response:** `200 OK`
```json
{
  "originalText": "string",
  "translatedText": "string",
  "timestamp": "ISO 8601 datetime",
  "processingTimeMs": "integer"
}
```

**Error Responses:**
- `400 Bad Request` - Invalid input
- `500 Internal Server Error` - Translation service error
- `503 Service Unavailable` - External service unavailable

---

### Lyrics API

**GET** `/api/lyrics`

**Response:** `200 OK`
```json
[
  {
    "id": 1,
    "title": "string",
    "artist": "string",
    "lyrics": "string",
    "isCockney": boolean,
    "createdDate": "ISO 8601",
    "lastModified": "ISO 8601"
  }
]
```

**POST** `/api/lyrics`

**Request Body:**
```json
{
  "title": "string (required, max 200)",
  "artist": "string (required, max 100)",
  "lyrics": "string (required, max 10000)",
  "isCockney": boolean
}
```

**Response:** `201 Created`

**PUT** `/api/lyrics/{id}`

**Request Body:** Same as POST

**Response:** `204 No Content`

**DELETE** `/api/lyrics/{id}`

**Response:** `204 No Content`

---

## Telemetry & Monitoring

### Custom Telemetry Events

#### 1. TranslationRequest
**Properties:**
- `InputLength`: Character count of input text
- `OutputLength`: Character count of translated text
- `Success`: Boolean success indicator
- `ProcessingTimeMs`: Translation duration
- `ErrorMessage`: Error details (if failed)

**Purpose**: Track translation usage, performance, and success rates

#### 2. LyricsAccess
**Properties:**
- `Operation`: Create | Read | Update | Delete
- `LyricsId`: ID of affected lyrics entry
- `Success`: Boolean success indicator
- `ErrorMessage`: Error details (if failed)

**Purpose**: Monitor lyrics CRUD operations and usage patterns

#### 3. AudioSynthesis
**Properties:**
- `TextLength`: Character count of input text
- `Voice`: Selected voice name
- `Success`: Boolean success indicator
- `AudioDurationSeconds`: Length of generated audio
- `ErrorMessage`: Error details (if failed)

**Purpose**: Track audio generation usage and performance

#### 4. DataUsage
**Properties:**
- `DataType`: Translation | Lyrics | Audio
- `OperationType`: Read | Write | Update | Delete
- `RecordCount`: Number of records affected
- `Success`: Boolean success indicator

**Purpose**: Analyze API usage patterns and data access

#### 5. PerformanceMetric
**Properties:**
- `MetricName`: Name of measured operation
- `Value`: Numeric metric value
- `Unit`: ms | requests | MB
- `Threshold`: Performance threshold value
- `ExceededThreshold`: Boolean indicator

**Purpose**: Track performance against SLA targets

#### 6. UserActivity
**Properties:**
- `ActivityType`: PageView | ButtonClick | FormSubmit
- `Page`: Current page/component name
- `SessionId`: User session identifier
- `Timestamp`: Activity timestamp

**Purpose**: Understand user engagement and navigation patterns

### KQL Query Categories

**User Activity** (`docs/KQL/01-UserActivity.kql`):
- Daily active users (DAU)
- Session duration analysis
- Activity heatmap by hour
- User retention rates
- Popular features

**Performance** (`docs/KQL/02-SlowRequests.kql`):
- P50/P95/P99 response times
- Top 10 slowest requests
- Response time trends
- Performance degradation detection
- Dependency call analysis

**Error Tracking** (`docs/KQL/03-ErrorRate.kql`):
- Overall error percentage
- Errors by endpoint
- HTTP status distribution
- Error spike detection
- Availability SLA calculation

**Custom Telemetry** (`docs/KQL/04-CustomTelemetry.kql`):
- Translation analytics (volume, success rate)
- Lyrics popularity (most accessed)
- Audio synthesis efficiency
- Data usage trends
- Performance threshold violations

---

## User Interface

### Pages

#### 1. Home/Translation Page
**Route:** `/`

**Components:**
- Translation input text area (2000 char limit)
- Translate button
- Loading spinner during API call
- Translation result display
- Character counter
- Error message display

**User Flow:**
1. Enter modern English text
2. Click "Translate" button
3. View Victorian English translation
4. Copy or clear result

#### 2. Lyrics Management Page
**Route:** `/lyrics`

**Components:**
- Lyrics data grid (Title, Artist, Cockney flag)
- Add New button
- Edit buttons per row
- Delete buttons per row
- Search/filter controls (future)
- Pagination controls (future)

**User Flow:**
1. View all lyrics in grid
2. Click "Add New" to create entry
3. Fill form (Title, Artist, Lyrics, Cockney flag)
4. Save or cancel
5. Edit existing entries
6. Delete entries with confirmation

#### 3. Audio Synthesis Page
**Route:** `/audio`

**Components:**
- Text input for synthesis
- Voice selection dropdown
- Synthesize button
- Audio player controls
- Download button
- Error message display

**User Flow:**
1. Enter text to synthesize
2. Select British English voice
3. Click "Synthesize"
4. Play audio in browser
5. Download audio file (optional)

---

## Deployment Strategy

### Environments

1. **Development**: Local developer machines
2. **Production**: Azure App Service (PoVicTranslate)

### CI/CD Pipeline

**Trigger**: Push to `master` branch

**Steps**:
1. **Checkout**: Clone repository
2. **Setup .NET**: Install .NET 9.0 SDK
3. **Restore**: Restore NuGet packages
4. **Build**: Compile solution
5. **Test**: Run unit and integration tests
6. **Publish**: Create deployment package
7. **Azure Login**: Authenticate via federated credentials
8. **Deploy**: Push to Azure App Service
9. **Verify**: Check health endpoint
10. **Logout**: Clean up Azure session

**GitHub Actions Workflow**: `.github/workflows/deploy.yml`

**Secrets Required**:
- `AZURE_CLIENT_ID`: App Registration client ID
- `AZURE_TENANT_ID`: Azure AD tenant ID
- `AZURE_SUBSCRIPTION_ID`: Target subscription

### Infrastructure as Code

**Bicep Templates**:
- `infra/main.bicep`: Subscription-level deployment
- `infra/resources.bicep`: Resource group resources

**Resources Provisioned**:
- App Service (Windows, .NET 9.0)
- App Service Plan (F1 Free tier)
- Application Insights
- Log Analytics Workspace

---

## Testing Strategy

### Unit Tests

**Framework**: xUnit 3.1.0

**Coverage Goals**: 80% line coverage, 70% branch coverage

**Test Categories**:
- Service layer (TranslationService, LyricsService, AudioSynthesisService)
- Controllers (TranslationController, LyricsController, AudioController)
- Health Checks (AzureOpenAIHealthCheck, AzureSpeechHealthCheck)
- Middleware (DebugLoggingMiddleware, ProblemDetailsExceptionHandler)
- Configuration (ConfigurationValidator)

**Mocking Strategy**:
- Azure SDK clients mocked with Moq
- HTTP clients mocked with MockHttpMessageHandler
- Logger mocked with Mock<ILogger>

### Integration Tests

**Framework**: xUnit 3.1.0 with WebApplicationFactory

**Scope**:
- Full HTTP request/response testing
- Database integration (future)
- External service integration (with test doubles)

**Test Scenarios**:
- Health endpoint returns 200 OK
- Translation endpoint with valid input
- Lyrics CRUD operations
- Error handling (4xx, 5xx responses)

### End-to-End Tests

**Framework**: Playwright (TypeScript)

**Browsers**: Chromium, Firefox, WebKit

**Test Scenarios**:
- Complete translation workflow
- Lyrics management workflow
- Audio synthesis workflow
- Error handling and validation
- Responsive design verification

**Reports**: HTML report in `tests/Po.VicTranslate.E2ETests/playwright-report/`

---

## Security Considerations

### Authentication & Authorization

**Current State**: No authentication (public API)

**Future Enhancements**:
- Azure AD B2C integration
- API key authentication for programmatic access
- Role-based access control (Admin, User, ReadOnly)

### Data Protection

- **API Keys**: Stored in Azure App Service Configuration
- **Secrets**: User Secrets (dev), Azure Key Vault (prod, future)
- **HTTPS**: Enforced with TLS 1.2+
- **CORS**: Restricted to allowed origins
- **Input Validation**: Prevent XSS, SQL injection, command injection

### Compliance

- **GDPR**: User data handling policies (future)
- **Logging**: No PII in application logs
- **Retention**: 30-day log retention in Application Insights

---

## Risk Assessment

### Technical Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Azure OpenAI quota exceeded | High | Medium | Implement request throttling, monitoring |
| External service downtime | High | Low | Circuit breaker, health checks, retry logic |
| Memory leak in application | Medium | Low | Regular monitoring, load testing |
| Breaking API changes | Medium | Low | API versioning, comprehensive testing |

### Business Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Low user adoption | High | Medium | Marketing, user feedback, feature iteration |
| Azure cost overruns | Medium | Medium | Cost monitoring, alerts, budget limits |
| Competitor solutions | Medium | Medium | Unique features, continuous improvement |
| Historical accuracy concerns | Medium | Low | Linguistic expert review, user feedback |

---

## Future Enhancements

### Phase 7: Advanced Features (Q1 2026)

- **User Accounts**: Azure AD B2C authentication
- **Favorites**: Save favorite translations and lyrics
- **History**: Track user translation history
- **Batch Operations**: Translate multiple texts at once
- **API Rate Limiting**: Protect against abuse

### Phase 8: Data Persistence (Q2 2026)

- **Azure Table Storage**: Persistent lyrics database
- **Cosmos DB**: Scalable NoSQL storage (alternative)
- **Caching**: Redis for frequently accessed data
- **Search**: Azure Cognitive Search for lyrics

### Phase 9: Advanced NLP (Q3 2026)

- **Dialect Selection**: Choose specific Victorian sub-dialects
- **Sentiment Analysis**: Analyze input text sentiment
- **Entity Recognition**: Identify names, places, dates
- **Translation Memory**: Learn from previous translations

### Phase 10: Mobile Apps (Q4 2026)

- **iOS**: Native Swift application
- **Android**: Native Kotlin application
- **MAUI**: Cross-platform .NET app
- **Offline Mode**: Local translation cache

---

## Glossary

- **Victorian English**: English language used during the Victorian era (1837-1901)
- **Cockney Rhyming Slang**: A form of English slang originating in East London
- **GPT-4**: Generative Pre-trained Transformer 4, OpenAI's large language model
- **Azure OpenAI**: Microsoft's managed service for OpenAI models
- **Blazor WebAssembly**: .NET client-side web framework running in the browser
- **Serilog**: Structured logging library for .NET
- **Application Insights**: Azure's application performance monitoring service
- **KQL**: Kusto Query Language for querying Azure Monitor logs
- **Health Check**: Endpoint that reports application and dependency status
- **Federated Credentials**: OIDC-based authentication without secrets

---

## Appendix

### A. Configuration Reference

See `src/Po.VicTranslate.Api/appsettings.json` for complete configuration schema.

### B. API Error Codes

| Code | Description | Resolution |
|------|-------------|------------|
| 400 | Bad Request | Check input validation |
| 404 | Not Found | Verify resource ID |
| 500 | Internal Server Error | Check Application Insights logs |
| 503 | Service Unavailable | External service down, retry later |

### C. Performance Benchmarks

Based on testing with 100 concurrent users:
- Translation: 1.2s average, 2.8s P95
- Lyrics Read: 150ms average, 300ms P95
- Audio Synthesis: 2.5s average, 4.5s P95

### D. Monitoring Dashboards

- **Azure Portal**: PoVicTranslate App Service Overview
- **Application Insights**: Performance and failure dashboards
- **Log Analytics**: Custom KQL query workbooks

---

**Document Version History**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-10-29 | Dev Team | Initial PRD creation |

---

**Approval**

This PRD represents the current state of the PoVicTranslate application as of Phase 6 completion.
