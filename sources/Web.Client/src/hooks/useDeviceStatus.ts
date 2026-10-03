import React from 'react';
import { devicesApi } from 'src/lib/api/devices/devicesApi';

interface IDeviceStatusState {
    status: 'loading' | 'ready';
    isPaired: boolean;
}

/** Asks the server whether this device is paired. A failed request counts as "not paired". */
export const useDeviceStatus = (): IDeviceStatusState => {
    const [state, setState] = React.useState<IDeviceStatusState>({
        status: 'loading',
        isPaired: false,
    });

    React.useEffect(() => {
        const controller = new AbortController();

        void devicesApi.status.get({ signal: controller.signal }).then((result) => {
            if (result.error?.kind === 'canceled') {
                return;
            }

            setState({ status: 'ready', isPaired: result.data?.isPaired ?? false });
        });

        return () => controller.abort();
    }, []);

    return state;
};
