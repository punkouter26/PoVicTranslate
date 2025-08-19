// Debug logging service for browser-side events
class BrowserDebugService {
    constructor(baseUrl = '') {
        this.baseUrl = baseUrl;
        this.sessionId = this.generateSessionId();
        this.isInitialized = false;
        this.logQueue = [];
        this.init();
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
        window.addEventListener('error', (event) => {
            this.logStructuralFailure('Browser', 'JavaScript Error', {
                message: event.message,
                filename: event.filename,
                lineno: event.lineno,
                colno: event.colno,
                error: event.error?.toString(),
                stack: event.error?.stack
            });
        });

        window.addEventListener('unhandledrejection', (event) => {
            this.logStructuralFailure('Browser', 'Unhandled Promise Rejection', {
                reason: event.reason?.toString(),
                stack: event.reason?.stack
            });
        });
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
}

// Initialize the debug service
window.browserDebugService = new BrowserDebugService();

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = BrowserDebugService;
}
