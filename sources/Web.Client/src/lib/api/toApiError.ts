import axios from 'axios';
import type { ApiError } from 'src/lib/api/types/apiError';
import type { ProblemDetails } from 'src/lib/api/types/problemDetails';
import type { NotificationKey } from 'src/lib/translations/translationKeys';

const isProblemDetails = (value: unknown): value is ProblemDetails =>
    typeof value === 'object' &&
    value !== null &&
    ('title' in value || 'status' in value || 'errors' in value);

const messageKeyForStatus = (status: number): NotificationKey => {
    switch (status) {
        case 400:
        case 422:
            return 'notificationInvalidInput';
        case 401:
            // apiClient already tried a refresh - the session is gone.
            return 'notificationSessionExpired';
        case 403:
            return 'notificationAccessDenied';
        case 404:
            return 'notificationNotFound';
        case 410:
            return 'notificationLinkExpired';
        case 423:
            return 'notificationAccountLocked';
        case 429:
            return 'notificationTooManyAttempts';
        default:
            return 'notificationUnexpectedError';
    }
};

/** Turns whatever axios threw into an ApiError with a translatable message. */
export const toApiError = (error: unknown): ApiError => {
    if (axios.isCancel(error)) {
        return { kind: 'canceled', messageKey: 'notificationUnexpectedError' };
    }

    if (!axios.isAxiosError(error)) {
        return { kind: 'unexpected', messageKey: 'notificationUnexpectedError' };
    }

    if (!error.response) {
        return { kind: 'network', messageKey: 'notificationNetworkError' };
    }

    const { status } = error.response;
    const data: unknown = error.response.data;

    return {
        kind: 'http',
        status,
        problem: isProblemDetails(data) ? data : undefined,
        messageKey: messageKeyForStatus(status),
    };
};
