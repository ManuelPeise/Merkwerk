export type QueryParams = Record<string, string | number | boolean | null | undefined>;

export type StatelessApiOptions<TRequest = undefined> = {
    /** Path below /api/v1, e.g. '/exercises' or '/exercises/42'. */
    serviceUrl: string;
    /** Query string parameters; undefined and null values are left out. */
    params?: QueryParams;
    /** Request body for POST and PUT, sent as JSON. */
    body?: TRequest;
    /** Cancels the request, e.g. from the cleanup of a useEffect. */
    signal?: AbortSignal;
};
