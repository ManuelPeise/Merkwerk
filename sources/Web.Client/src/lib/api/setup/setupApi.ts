import { statelessApi } from 'src/lib/api/StatelessApi';
import type { ISession } from 'src/lib/api/authentication/authenticationTypes';
import type { ISetupInitializeRequest, ISetupStatus } from 'src/lib/api/setup/setupTypes';

/** Endpoints of SetupController - only usable while the instance has no users. */
export const setupApi = {
    status: statelessApi.create<ISetupStatus>({ serviceUrl: '/setup/status' }),
    initialize: statelessApi.create<ISession, ISetupInitializeRequest>({
        serviceUrl: '/setup/initialize',
    }),
};
