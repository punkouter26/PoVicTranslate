// Debug logging service for browser-side events
class BrowserDebugService {
    constructor(baseUrl = '') {
        this.baseUrl = baseUrl;
        this.sessionId = this.generateSessionId();
        this.isInitialized = false;
        this.logQueue = [];
        // Detect E2E test mode - disable aggressive logging
        this.isE2ETest = this.detectE2ETestMode();
        this.init();
    }

    detectE2ETestMode() {
        // Detect Playwright/E2E test environment
        return navigator.webdriver === true || 
               window.location.search.includes('e2e=true') ||
               window.__playwright !== undefined;
    }

    generateSessionId() {
        return Math.random().toString(36).substring(2, 10);
    }

    async init() {
        try {
            // Override console methods to capture logs
            this.interceptConsole();
            
            // Capture unhandled errors
            this.captureErrors();
            
            // Capture page events
            this.capturePageEvents();

            await this.logEvent('BrowserDebugInit', 'Browser debug service initialized', {
                sessionId: this.sessionId,
                userAgent: navigator.userAgent,
                url: window.location.href,
                timestamp: new Date().toISOString()
            });

            this.isInitialized = true;
            console.log('🔍 Browser Debug Service initialized');
        } catch (error) {
            console.error('Failed to initialize browser debug service:', error);
        }
    }

    interceptConsole() {
        // Skip console interception in E2E test mode
        if (this.isE2ETest) {
            console.log('[E2E Mode] Console interception disabled');
            return;
        }
        
        const originalConsole = { ...console };
        
        ['log', 'info', 'warn', 'error', 'debug'].forEach(level => {
            console[level] = (...args) => {
                // Call original console method
                originalConsole[level].apply(console, args);
                
                // Send to debug service
                this.logBrowserEvent('Console', level, args.join(' '), {
                    level: level,
                    args: args,
                    stackTrace: new Error().stack
                });
            };
        });
    }

    captureErrors() {
        // Enhanced error capturing with Blazor-specific handling
        window.addEventListener('error', (event) => {
            console.error('🚨 JavaScript Error Captured:', {
                message: event.message,
                filename: event.filename,
                lineno: event.lineno,
                colno: event.colno,
                error: event.error,
                stack: event.error?.stack
            });
            
            this.logStructuralFailure('Browser', 'JavaScript Error', {
                message: event.message,
                filename: event.filename,
                lineno: event.lineno,
                colno: event.colno,
                error: event.error?.toString(),
                stack: event.error?.stack,
                blazorErrorUI: document.getElementById('blazor-error-ui')?.style.display
            });
        });

        window.addEventListener('unhandledrejection', (event) => {
            console.error('🚨 Unhandled Promise Rejection:', {
                reason: event.reason,
                stack: event.reason?.stack
            });
            
            this.logStructuralFailure('Browser', 'Unhandled Promise Rejection', {
                reason: event.reason?.toString(),
                stack: event.reason?.stack,
                blazorErrorUI: document.getElementById('blazor-error-ui')?.style.display
            });
        });

        // Blazor-specific error monitoring
        this.monitorBlazorErrorUI();
        
        // Monitor for .NET runtime errors
        this.monitorDotNetErrors();
    }

    capturePageEvents() {
        // Page load events
        window.addEventListener('load', () => {
            this.logEvent('PageLoad', 'Page fully loaded', {
                loadTime: performance.now(),
                url: window.location.href
            });
        });

        // Navigation events
        window.addEventListener('beforeunload', () => {
            this.logEvent('PageUnload', 'Page unloading', {
                url: window.location.href,
                sessionDuration: performance.now()
            });
        });

        // Performance monitoring
        if ('PerformanceObserver' in window) {
            try {
                const observer = new PerformanceObserver((list) => {
                    list.getEntries().forEach(entry => {
                        if (entry.entryType === 'navigation') {
                            this.logEvent('PerformanceMetric', 'Navigation timing', {
                                type: entry.entryType,
                                duration: entry.duration,
                                loadEventEnd: entry.loadEventEnd,
                                domContentLoadedEventEnd: entry.domContentLoadedEventEnd
                            });
                        }
                    });
                });
                observer.observe({ entryTypes: ['navigation'] });
            } catch (e) {
                console.warn('Performance observer not supported:', e);
            }
        }
    }

    async logEvent(eventType, message, data = null) {
        await this.sendToServer('event', {
            eventType,
            message,
            data
        });
    }

    async logBrowserEvent(component, level, message, data = null) {
        await this.sendToServer('browser-event', {
            eventType: 'BrowserEvent',
            component,
            level,
            message,
            data
        });
    }

    async logInstability(component, issue, diagnosticData = null) {
        await this.sendToServer('instability', {
            component,
            issue,
            diagnosticData
        });
    }

    async logStructuralFailure(component, failure, context = null) {
        await this.sendToServer('failure', {
            component,
            failure,
            context
        });
    }

    async sendToServer(type, payload) {
        // Skip sending logs during E2E tests to avoid network activity
        if (this.isE2ETest) {
            console.log('[E2E Mode] Skipping debug log:', type, payload);
            return;
        }
        
        if (!this.isInitialized && type !== 'event') {
            this.logQueue.push({ type, payload });
            return;
        }

        try {
            const response = await fetch(`${this.baseUrl}/api/debug/browser-log`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    type,
                    payload: {
                        ...payload,
                        sessionId: this.sessionId,
                        timestamp: new Date().toISOString(),
                        url: window.location.href
                    }
                })
            });

            if (!response.ok) {
                console.warn('Failed to send debug log to server:', response.statusText);
            }
        } catch (error) {
            console.warn('Failed to send debug log to server:', error);
        }
    }

    // Performance monitoring methods
    measureOperation(name, operation) {
        const startTime = performance.now();
        try {
            const result = operation();
            if (result instanceof Promise) {
                return result.finally(() => {
                    const duration = performance.now() - startTime;
                    this.logEvent('PerformanceMetric', `Operation: ${name}`, {
                        operationName: name,
                        duration: duration,
                        success: true
                    });
                });
            } else {
                const duration = performance.now() - startTime;
                this.logEvent('PerformanceMetric', `Operation: ${name}`, {
                    operationName: name,
                    duration: duration,
                    success: true
                });
                return result;
            }
        } catch (error) {
            const duration = performance.now() - startTime;
            this.logEvent('PerformanceMetric', `Operation: ${name} (failed)`, {
                operationName: name,
                duration: duration,
                success: false,
                error: error.toString()
            });
            throw error;
        }
    }

    // Get system information
    getSystemInfo() {
        return {
            userAgent: navigator.userAgent,
            language: navigator.language,
            platform: navigator.platform,
            cookieEnabled: navigator.cookieEnabled,
            onLine: navigator.onLine,
            screen: {
                width: screen.width,
                height: screen.height,
                colorDepth: screen.colorDepth
            },
            viewport: {
                width: window.innerWidth,
                height: window.innerHeight
            },
            memory: performance.memory ? {
                usedJSHeapSize: performance.memory.usedJSHeapSize,
                totalJSHeapSize: performance.memory.totalJSHeapSize,
                jsHeapSizeLimit: performance.memory.jsHeapSizeLimit
            } : null
        };
    }

    // Manual logging methods for specific events
    logPageInteraction(elementType, action, details = {}) {
        this.logEvent('UserInteraction', `${action} on ${elementType}`, {
            elementType,
            action,
            ...details,
            systemInfo: this.getSystemInfo()
        });
    }

    logApiCall(url, method, duration, success, statusCode = null) {
        this.logEvent('ApiCall', `${method} ${url}`, {
            url,
            method,
            duration,
            success,
            statusCode
        });
    }

    logComponentLifecycle(componentName, lifecycle, data = {}) {
        this.logEvent('ComponentLifecycle', `${componentName}: ${lifecycle}`, {
            componentName,
            lifecycle,
            ...data
        });
    }

    monitorBlazorErrorUI() {
        // Monitor when Blazor error UI becomes visible
        const errorUI = document.getElementById('blazor-error-ui');
        if (errorUI) {
            const observer = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    if (mutation.type === 'attributes' && mutation.attributeName === 'style') {
                        const display = errorUI.style.display;
                        const isVisible = display !== 'none' && display !== '';
                        
                        if (isVisible) {
                            console.error('🚨 Blazor Error UI became visible!', {
                                display: display,
                                innerHTML: errorUI.innerHTML,
                                computedStyle: window.getComputedStyle(errorUI).display
                            });
                            
                            this.logStructuralFailure('Blazor', 'Error UI Visible', {
                                display: display,
                                computedDisplay: window.getComputedStyle(errorUI).display,
                                errorUIContent: errorUI.innerHTML,
                                timestamp: new Date().toISOString()
                            });
                        }
                    }
                });
            });
            
            observer.observe(errorUI, {
                attributes: true,
                attributeFilter: ['style', 'class']
            });
            
            // Also check initial state
            const isInitiallyVisible = window.getComputedStyle(errorUI).display !== 'none';
            if (isInitiallyVisible) {
                console.warn('🚨 Blazor Error UI is initially visible!');
                this.logStructuralFailure('Blazor', 'Error UI Initially Visible', {
                    computedDisplay: window.getComputedStyle(errorUI).display,
                    errorUIContent: errorUI.innerHTML
                });
            }
        }
    }

    monitorDotNetErrors() {
        // Monitor for .NET WebAssembly runtime errors
        if (window.DotNet) {
            console.log('🔍 Monitoring .NET runtime...');
            
            // Override DotNet error reporting if available
            const originalInvokeMethod = window.DotNet.invokeMethod;
            if (originalInvokeMethod) {
                window.DotNet.invokeMethod = function(...args) {
                    try {
                        return originalInvokeMethod.apply(this, args);
                    } catch (error) {
                        console.error('🚨 .NET Method Invocation Error:', error);
                        window.browserDebugService?.logStructuralFailure('DotNet', 'Method Invocation Error', {
                            args: args,
                            error: error.toString(),
                            stack: error.stack
                        });
                        throw error;
                    }
                };
            }
        }
        
        // Monitor Blazor WebAssembly startup
        window.addEventListener('blazor:started', () => {
            console.log('✅ Blazor WebAssembly started successfully');
            this.logEvent('Blazor', 'WebAssembly Started', {
                timestamp: new Date().toISOString()
            });
        });
        
        window.addEventListener('blazor:error', (event) => {
            console.error('🚨 Blazor Error Event:', event.detail);
            this.logStructuralFailure('Blazor', 'Runtime Error', {
                detail: event.detail,
                timestamp: new Date().toISOString()
            });
        });
    }
}

// Initialize the debug service
window.browserDebugService = new BrowserDebugService();

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = BrowserDebugService;
}
