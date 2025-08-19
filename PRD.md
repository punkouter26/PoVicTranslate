# Product Requirements Document (PRD) - PoVicTranslate

## Application Overview

### Product Vision
PoVicTranslate is a sophisticated web application that transforms modern text into authentic Victorian-era English using advanced AI translation capabilities. The application combines cutting-edge Azure AI services with a user-friendly interface to deliver high-quality historical language translation with optional text-to-speech functionality.

### Core Purpose
To provide users with an entertaining and educational tool that converts contemporary language into the elaborate, formal style of the Victorian era, complete with period-appropriate vocabulary, expressions, and linguistic structures.

### Key Features
- **AI-Powered Translation**: Leverages Azure OpenAI GPT-4o for context-aware Victorian English translation
- **Text-to-Speech Integration**: Azure Speech Services with Victorian-appropriate British voices
- **Lyrics Library**: Pre-loaded collection of rap/hip-hop lyrics for translation experimentation
- **Real-time Debugging**: Comprehensive logging and monitoring system for development and production
- **Responsive Design**: Modern web interface with urban/hip-hop inspired styling
- **Copy-to-Clipboard**: Easy sharing of translated content
- **Word Count Validation**: 200-word limit to ensure optimal translation quality

### Target Audience
- **Language Enthusiasts**: Individuals interested in historical linguistics and language evolution
- **Content Creators**: Writers, educators, and entertainers seeking period-appropriate language
- **Students and Educators**: Those studying Victorian literature, history, or linguistics
- **Hobbyists**: Users interested in experimenting with text transformation and AI capabilities

### Technology Stack
- **Frontend**: Blazor WebAssembly (.NET 9)
- **Backend**: ASP.NET Core Web API (.NET 9)
- **AI Services**: Azure OpenAI (GPT-4o), Azure Speech Services
- **Logging**: Serilog with structured logging
- **Monitoring**: Application Insights integration
- **Architecture**: Clean Architecture principles with Vertical Slice patterns

---

## UI Pages & Components

### 1. Translation Page (`/` or `/translate`)
**Primary Interface** - Main user interaction hub

#### UI Components:
- **Header Section**: Application title "PoVicTranslate" with Victorian styling
- **Input Area**:
  - Large textarea for text input (5 rows)
  - Real-time word counter (200-word limit)
  - Visual indicators for word limit enforcement
  - Placeholder text: "Type or paste your lyrics here..."
- **Lyrics Library Integration**:
  - Dropdown selector for pre-loaded rap/hip-hop lyrics
  - "Load Random Lyrics" functionality
  - Song title display and metadata
- **Translation Controls**:
  - "Make It Victorian" primary action button
  - Loading spinner with "Translating..." status
  - Disabled state management during processing
- **Results Display**:
  - Formatted output area with Victorian styling
  - Copy-to-clipboard button with visual feedback
  - "Speak It Proper" text-to-speech button
  - Audio playback controls and status indicators
- **Error Handling**:
  - Contextual error messages
  - Graceful fallback for service failures
  - User-friendly validation feedback

#### Styling Features:
- **Urban/Hip-Hop Theme**: Gold (#ffd700) and dark backgrounds
- **Victorian Accents**: Elegant typography and decorative elements
- **Responsive Layout**: Mobile-first design approach
- **Interactive Elements**: Hover effects and smooth transitions
- **Accessibility**: WCAG compliant color contrast and keyboard navigation

### 2. Diagnostics Page (`/diag`)
**System Health Monitoring** - Technical status and debugging interface

#### UI Components:
- **Connection Status Panel**:
  - API connectivity indicators
  - Service health checks
  - Real-time status updates
- **System Information Display**:
  - Application version information
  - Environment details (Development/Production)
  - API endpoint configurations
- **Debug Log Access**:
  - Recent log entries display
  - Error tracking and analysis
  - Performance metrics visualization
- **Service Testing Interface**:
  - Manual API endpoint testing
  - Service validation controls
  - Response time monitoring

#### Technical Features:
- **Real-time Health Checks**: Continuous monitoring of critical dependencies
- **Log Integration**: Direct access to structured debug logs
- **Performance Metrics**: Response time and resource usage tracking
- **Error Tracking**: Centralized error reporting and analysis

### 3. Error Page (`/error`)
**Error Handling** - User-friendly error recovery interface

#### UI Components:
- **Error Information Display**:
  - User-friendly error messages
  - Technical details (when appropriate)
  - Suggested recovery actions
- **Navigation Controls**:
  - Return to main application
  - Refresh/retry functionality
  - Contact information for support

### 4. Shared Components

#### Layout Structure:
- **App.razor**: Root application component with global styling
- **Routes.razor**: Client-side routing configuration
- **_Imports.razor**: Shared namespace imports

#### Navigation Elements:
- **Responsive Navigation**: Seamless routing between pages
- **Header/Footer**: Consistent branding and navigation
- **Loading States**: Global loading indicators and progress feedback

### 5. Component Architecture

#### Client Services:
- **ClientTranslationService**: API communication for translation requests
- **ClientLyricsService**: Lyrics library management and retrieval
- **ClientSpeechService**: Text-to-speech functionality coordination

#### State Management:
- **Local Component State**: Individual component data management
- **Error State Handling**: Centralized error processing
- **Loading State Coordination**: UI feedback during async operations

#### Integration Points:
- **JavaScript Interop**: Audio playback and browser API access
- **Debug Logging**: Client-side event tracking and error reporting
- **API Communication**: RESTful service integration with error handling

### User Experience Flow

1. **Entry Point**: User navigates to main translation page
2. **Input Phase**: User enters text manually or selects from lyrics library
3. **Validation**: Real-time word count validation and input sanitization
4. **Translation**: AI-powered conversion with loading feedback
5. **Results**: Formatted Victorian English output with interaction options
6. **Audio (Optional)**: Text-to-speech playback with British Victorian voice
7. **Sharing**: Copy-to-clipboard functionality for easy sharing
8. **Iteration**: User can modify input and retranslate seamlessly

### Accessibility & Performance
- **WCAG 2.1 AA Compliance**: Keyboard navigation, screen reader support
- **Progressive Web App Features**: Offline capability considerations
- **Performance Optimization**: Lazy loading, efficient rendering
- **Mobile Responsiveness**: Touch-friendly interface design
- **Error Recovery**: Graceful degradation and user guidance
