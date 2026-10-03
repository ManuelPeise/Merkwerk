import React from 'react';
import { setupApi } from 'src/lib/api/setup/setupApi';

interface ISetupStatusState {
    status: 'loading' | 'ready';
    isSetupRequired: boolean;
}

interface IUseSetupStatus extends ISetupStatusState {
    markSetupDone: () => void;
}

/**
 * Asks once whether the instance still needs its first setup. If the status can't be read
 * (backend not ready, network), the app is not locked out: setup counts as not required.
 */
export const useSetupStatus = (): IUseSetupStatus => {
    const [state, setState] = React.useState<ISetupStatusState>({
        status: 'loading',
        isSetupRequired: false,
    });

    React.useEffect(() => {
        const controller = new AbortController();

        void setupApi.status.get({ signal: controller.signal }).then((result) => {
            if (result.error?.kind === 'canceled') {
                return;
            }

            setState({ status: 'ready', isSetupRequired: result.data?.isSetupRequired ?? false });
        });

        return () => controller.abort();
    }, []);

    const markSetupDone = React.useCallback(
        () => setState({ status: 'ready', isSetupRequired: false }),
        [],
    );

    return { ...state, markSetupDone };
};
