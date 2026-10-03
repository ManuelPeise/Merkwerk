import type { ApiError } from 'src/lib/api/types/apiError';
import type { StatelessApiOptions } from 'src/lib/api/types/statelessApiOptions';

/** Either data or error – requests never throw. */
export type StatelessApiResponse<TResponse> =
    { data: TResponse; error?: never } | { data?: never; error: ApiError };

/** Per call, options can override the ones given to create() (e.g. params, body, signal). */
export type StatelessApiRequest<TResponse, TRequest> = (
    options?: Partial<StatelessApiOptions<TRequest>>,
) => Promise<StatelessApiResponse<TResponse>>;

export type StatelessApiResult<TResponse, TRequest = undefined> = {
    get: StatelessApiRequest<TResponse, TRequest>;
    post: StatelessApiRequest<TResponse, TRequest>;
};
