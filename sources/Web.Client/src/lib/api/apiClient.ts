import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';

/**
 * Single axios instance for Web.Core. Auth lives in HttpOnly cookies (mw_access / mw_refresh),
 * so the client never touches tokens: it only sends cookies and refreshes once on 401.
 */
export const apiClient = axios.create({
    baseURL: '/api/v1',
    withCredentials: true,
    headers: { Accept: 'application/json' },
});

/** AuthenticationController. Lowercase on purpose: the refresh cookie's path is case-sensitive. */
const refreshPath = '/authentication/refresh';

/** A 401 from these means wrong credentials or no session – refreshing won't help. */
const noRefreshPaths = ['/authentication/login', refreshPath, '/authentication/logout'];

type RetriableRequest = InternalAxiosRequestConfig & { retried?: boolean };

let refreshInFlight: Promise<void> | null = null;

const sessionEvents = new EventTarget();
const sessionExpiredEvent = 'session-expired';

/** Called when a refresh fails, i.e. the session is gone. Returns the unsubscribe function. */
export const onSessionExpired = (listener: () => void): (() => void) => {
    sessionEvents.addEventListener(sessionExpiredEvent, listener);
    return () => sessionEvents.removeEventListener(sessionExpiredEvent, listener);
};

/** Parallel 401s share one refresh call (single flight). */
const refreshSession = (): Promise<void> => {
    refreshInFlight ??= apiClient
        .post(refreshPath)
        .then(() => undefined)
        .finally(() => {
            refreshInFlight = null;
        });

    return refreshInFlight;
};

apiClient.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
        const request = error.config as RetriableRequest | undefined;

        if (
            error.response?.status !== 401 ||
            !request ||
            request.retried ||
            noRefreshPaths.includes(request.url ?? '')
        ) {
            throw error;
        }

        request.retried = true;

        try {
            await refreshSession();
        } catch {
            sessionEvents.dispatchEvent(new Event(sessionExpiredEvent));
            throw error;
        }

        return apiClient(request);
    },
);
