// LP-005 spike: milliseconds since the browser started loading the page (navigation start).
export function msSinceNavigationStart() {
    return performance.now();
}

// LP-007: device facts for the spike results table.
export function deviceInfo() {
    return {
        userAgent: navigator.userAgent,
        screen: `${screen.width}x${screen.height} @${window.devicePixelRatio}x`,
        maxTouchPoints: navigator.maxTouchPoints,
        standalone: window.matchMedia('(display-mode: standalone)').matches,
    };
}
