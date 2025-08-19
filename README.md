# PoVicTranslate 🎩

> Transform modern text into authentic Victorian-era English using advanced AI technology

[![.NET 9](https://img.shields.io/badge/.NET-9-purple)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Blazor WebAssembly](https://img.shields.io/badge/Blazor-WebAssembly-blue)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![Azure OpenAI](https://img.shields.io/badge/Azure-OpenAI-green)](https://azure.microsoft.com/products/ai-services/openai-service)
[![Azure Speech](https://img.shields.io/badge/Azure-Speech-orange)](https://azure.microsoft.com/products/ai-services/speech-to-text)

## Summary

PoVicTranslate is a sophisticated web application that bridges the gap between modern language and Victorian-era English. Using Azure OpenAI's GPT-4o model, the application provides context-aware translation of contemporary text into authentic Victorian English, complete with period-appropriate vocabulary, expressions, and linguistic structures.

### 🚀 Key Features

- **AI-Powered Translation**: Leverages Azure OpenAI GPT-4o for intelligent Victorian English conversion
- **Text-to-Speech**: Azure Speech Services with authentic British Victorian voices
- **Lyrics Library**: Pre-loaded collection of rap/hip-hop lyrics for experimentation
- **Real-time Debugging**: Comprehensive logging system with browser integration
- **Modern UI**: Responsive design with urban styling and Victorian accents
- **Word Validation**: 200-word limit optimization for translation quality
- **Copy & Share**: Easy clipboard integration for sharing translations

### 🎯 Use Cases

- **Educational**: Teaching Victorian literature and historical linguistics
- **Content Creation**: Writers and educators seeking period-appropriate language
- **Entertainment**: Fun transformation of modern text for social media
- **Research**: Linguistic analysis and language evolution studies

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- Azure account with OpenAI and Speech Services
- Git for version control

### 🔧 Installation

1. **Clone the Repository**
   ```bash
   git clone https://github.com/punkouter25/PoVicTranslate.git
   cd PoVicTranslate
   ```

2. **Configure Azure Services**
   
   Update `VictorianTranslator.Server/appsettings.Development.json`:
   ```json
   {
     "ApiSettings": {
       "AzureOpenAIApiKey": "your-openai-api-key",
       "AzureOpenAIEndpoint": "https://your-openai-endpoint.openai.azure.com/",
       "AzureOpenAIDeploymentName": "gpt-4o",
       "AzureSpeechSubscriptionKey": "your-speech-api-key",
       "AzureSpeechRegion": "your-region"
     }
   }
   ```

3. **Restore Dependencies**
   ```bash
   dotnet restore PoVicTranslate.sln
   ```

4. **Build the Solution**
   ```bash
   dotnet build PoVicTranslate.sln
   ```

5. **Run the Application**
   ```bash
   cd VictorianTranslator.Server
   dotnet watch run
   ```

6. **Access the Application**
   - Open your browser to `https://localhost:5001`
   - The API documentation is available at `https://localhost:5001/swagger`

### 🚀 Quick Start Guide

1. **Navigate to the Main Page**: Open the application in your browser
2. **Enter Text**: Type or paste modern text into the input area (max 200 words)
3. **Alternative**: Select pre-loaded lyrics from the dropdown menu
4. **Translate**: Click "Make It Victorian" to transform your text
5. **Listen**: Use "Speak It Proper" to hear the Victorian translation
6. **Share**: Copy the result to clipboard for easy sharing

### 🔍 Testing the API

Use the provided debug scripts to test the API endpoints:

**PowerShell (Windows):**
```powershell
cd DEBUG
.\test-debug-api.ps1
```

**Bash (Linux/Mac/WSL):**
```bash
cd DEBUG
chmod +x test-debug-api.sh
./test-debug-api.sh
```

---

## Key Connections

### 🏗️ Architecture Overview

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Blazor WASM   │◄──►│   ASP.NET Core   │◄──►│  Azure Services │
│     Client      │    │    Web API       │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### 🔗 Service Integrations

#### **Azure OpenAI Integration**
- **Purpose**: Victorian English translation engine
- **Model**: GPT-4o deployment
- **Endpoint**: `POST /Translation/translate`
- **Configuration**: API key, endpoint, deployment name
- **Features**: Context-aware translation with period-specific prompts

#### **Azure Speech Services Integration**
- **Purpose**: Text-to-speech for Victorian translations
- **Voice**: `en-GB-RyanNeural` (British male, Victorian-appropriate)
- **Endpoint**: `POST /Speech/synthesize`
- **Configuration**: Subscription key, region
- **Output**: MP3 audio streamed to browser

#### **Application Insights Integration**
- **Purpose**: Performance monitoring and telemetry
- **Data Collection**: Request tracking, dependency monitoring, custom events
- **Configuration**: Connection string in appsettings
- **Integration**: Automatic ASP.NET Core instrumentation

### 🛠️ Technical Stack Connections

#### **Frontend Architecture**
- **Blazor WebAssembly**: Client-side SPA framework
- **Bootstrap 5**: UI component library and responsive design
- **JavaScript Interop**: Audio playback and browser API access
- **HTTP Client**: RESTful API communication with error handling

#### **Backend Architecture**
- **ASP.NET Core**: Web API hosting and middleware pipeline
- **Dependency Injection**: Service registration and lifecycle management
- **Serilog**: Structured logging with file and console outputs
- **Swagger/OpenAPI**: API documentation and testing interface

#### **Data & Storage**
- **In-Memory State**: Session-based data management
- **File System**: Lyrics library storage (`scrapes/` folder)
- **Debug Logs**: Structured JSON logs in `DEBUG/logs/`
- **Reports**: Summary reports in `DEBUG/reports/`

### 🔧 Development Workflow Connections

#### **Debug System Integration**
- **Server Middleware**: Automatic HTTP request/response logging
- **Client JavaScript**: Browser console capture and error tracking
- **API Endpoints**: Debug log retrieval and system health checks
- **File Structure**: Organized logs and reports for analysis

#### **Error Handling Chain**
```
Browser Error → Client Service → API Controller → Business Logic → Azure Service
     ↓              ↓              ↓              ↓              ↓
Debug Logger ← Client Logger ← Middleware ← Service Logger ← Azure Logs
```

#### **Monitoring & Observability**
- **Application Insights**: Centralized telemetry and performance monitoring
- **Structured Logging**: Serilog with contextual information
- **Health Checks**: Service availability and dependency status
- **Debug API**: Real-time log access and system diagnostics

### 🌐 Deployment Connections

#### **Azure Resource Dependencies**
- **App Service**: Application hosting and scaling
- **OpenAI Service**: AI translation capabilities
- **Speech Service**: Text-to-speech functionality
- **Application Insights**: Monitoring and analytics

#### **Configuration Management**
- **Development**: `appsettings.Development.json` with local secrets
- **Production**: Azure App Service Application Settings
- **Environment Variables**: Secure configuration for sensitive data
- **Feature Flags**: Environment-specific functionality control

### 📡 API Endpoint Mapping

| Endpoint | Purpose | Azure Service | Client Component |
|----------|---------|---------------|------------------|
| `POST /Translation/translate` | Text translation | Azure OpenAI | ClientTranslationService |
| `POST /Speech/synthesize` | Text-to-speech | Azure Speech | ClientSpeechService |
| `GET /Lyrics/available` | Lyrics library | File System | ClientLyricsService |
| `GET /Lyrics/{filename}` | Lyrics content | File System | ClientLyricsService |
| `GET /api/debug/*` | Debug operations | Debug System | Browser Debug Service |
| `GET /healthz` | Health checks | Internal | Diagnostics Page |

### 🔄 Data Flow Connections

1. **User Input** → Client Validation → API Request → Azure OpenAI → Response Processing → UI Update
2. **Audio Playback** → Speech API → Azure Speech Service → Audio Stream → Browser Playback
3. **Error Tracking** → Client Logger → API Endpoint → Debug Service → Log Files → Analysis Tools
4. **Performance Monitoring** → Application Insights → Azure Portal → Dashboards → Alerts

This interconnected architecture ensures reliable, scalable, and maintainable operation while providing comprehensive observability and debugging capabilities.
