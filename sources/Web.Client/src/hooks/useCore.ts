import { statelessApi } from 'src/lib/api/StatelessApi';
import type { StatelessApiOptions } from 'src/lib/api/types/statelessApiOptions';

type UseCoreResult = {
    createStatelessApi: (
        options: StatelessApiOptions<unknown>,
    ) => ReturnType<typeof statelessApi.create>;
};

export const useCore = (): UseCoreResult => {
    return {
        createStatelessApi: (options: StatelessApiOptions<unknown>) => statelessApi.create(options),
    };
};
