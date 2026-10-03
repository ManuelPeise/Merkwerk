import type { Method } from 'axios';
import { apiClient } from 'src/lib/api/apiClient';
import { toApiError } from 'src/lib/api/toApiError';
import type { StatelessApiOptions } from 'src/lib/api/types/statelessApiOptions';
import type {
    StatelessApiRequest,
    StatelessApiResponse,
    StatelessApiResult,
} from 'src/lib/api/types/statelessApiResult';

class StatelessApi {
    public create<TResponse, TRequest = undefined>(
        options: StatelessApiOptions<TRequest>,
    ): StatelessApiResult<TResponse, TRequest> {
        const send =
            (method: Method): StatelessApiRequest<TResponse, TRequest> =>
            (overrides) =>
                this.request<TResponse, TRequest>(method, { ...options, ...overrides });

        return {
            get: send('GET'),
            post: send('POST'),
        };
    }

    private async request<TResponse, TRequest>(
        method: Method,
        options: StatelessApiOptions<TRequest>,
    ): Promise<StatelessApiResponse<TResponse>> {
        try {
            const response = await apiClient.request<TResponse>({
                method,
                url: options.serviceUrl,
                params: options.params,
                data: options.body,
                signal: options.signal,
            });

            return { data: response.data };
        } catch (error) {
            return { error: toApiError(error) };
        }
    }
}

export const statelessApi = new StatelessApi();
