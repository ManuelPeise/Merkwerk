// LP-005 spike: milliseconds since the browser started loading the page (navigation start).
export function msSinceNavigationStart() {
    return performance.now();
}
