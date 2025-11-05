// Swipe gesture handler for mobile interactions
class SwipeHandler {
    constructor() {
        this.touchStartX = 0;
        this.touchStartY = 0;
        this.touchEndX = 0;
        this.touchEndY = 0;
        this.minSwipeDistance = 50; // minimum distance for a swipe
        this.handlers = new Map();
    }

    register(elementId, callbacks) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.warn(`Element with ID ${elementId} not found`);
            return;
        }

        const handler = {
            element,
            callbacks,
            touchStart: (e) => this.handleTouchStart(e),
            touchEnd: (e) => this.handleTouchEnd(e, callbacks)
        };

        element.addEventListener('touchstart', handler.touchStart, { passive: true });
        element.addEventListener('touchend', handler.touchEnd, { passive: true });

        this.handlers.set(elementId, handler);
    }

    unregister(elementId) {
        const handler = this.handlers.get(elementId);
        if (handler) {
            handler.element.removeEventListener('touchstart', handler.touchStart);
            handler.element.removeEventListener('touchend', handler.touchEnd);
            this.handlers.delete(elementId);
        }
    }

    handleTouchStart(e) {
        this.touchStartX = e.changedTouches[0].screenX;
        this.touchStartY = e.changedTouches[0].screenY;
    }

    handleTouchEnd(e, callbacks) {
        this.touchEndX = e.changedTouches[0].screenX;
        this.touchEndY = e.changedTouches[0].screenY;
        this.handleSwipe(callbacks);
    }

    handleSwipe(callbacks) {
        const diffX = this.touchEndX - this.touchStartX;
        const diffY = this.touchEndY - this.touchStartY;

        // Determine if horizontal or vertical swipe
        if (Math.abs(diffX) > Math.abs(diffY)) {
            // Horizontal swipe
            if (Math.abs(diffX) > this.minSwipeDistance) {
                if (diffX > 0) {
                    callbacks.onSwipeRight?.();
                } else {
                    callbacks.onSwipeLeft?.();
                }
            }
        } else {
            // Vertical swipe
            if (Math.abs(diffY) > this.minSwipeDistance) {
                if (diffY > 0) {
                    callbacks.onSwipeDown?.();
                } else {
                    callbacks.onSwipeUp?.();
                }
            }
        }
    }

    cleanup() {
        this.handlers.forEach((handler, elementId) => {
            this.unregister(elementId);
        });
    }
}

// Create global instance
window.swipeHandler = new SwipeHandler();

// Helper function for Blazor interop
window.registerSwipeHandler = (elementId, dotNetHelper, methodName) => {
    window.swipeHandler.register(elementId, {
        onSwipeLeft: () => dotNetHelper.invokeMethodAsync(methodName, 'left'),
        onSwipeRight: () => dotNetHelper.invokeMethodAsync(methodName, 'right'),
        onSwipeUp: () => dotNetHelper.invokeMethodAsync(methodName, 'up'),
        onSwipeDown: () => dotNetHelper.invokeMethodAsync(methodName, 'down')
    });
};

window.unregisterSwipeHandler = (elementId) => {
    window.swipeHandler.unregister(elementId);
};
