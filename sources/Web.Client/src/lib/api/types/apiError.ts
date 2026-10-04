import type { ProblemDetails } from 'src/lib/api/types/problemDetails';
import type { NotificationKey } from 'src/lib/translations/translationKeys';

/**
 * - `network`: no response (offline, server down)
 * - `http`: the server answered with an error status
 * - `canceled`: aborted via AbortSignal - usually ignore it
 * - `unexpected`: anything else (bug, invalid response)
 */
export type ApiErrorKind = 'network' | 'http' | 'canceled' | 'unexpected';

export type ApiError = {
    kind: ApiErrorKind;
    /** HTTP status, only for kind `http`. */
    status?: number;
    /** Error body from the server, if it sent ProblemDetails. */
    problem?: ProblemDetails;
    /** Text to show the user: t(error.messageKey). Never show raw server messages. */
    messageKey: NotificationKey;
};
