import { statelessApi } from 'src/lib/api/StatelessApi';
import type { ISession } from 'src/lib/api/authentication/authenticationTypes';
import type {
    IDevice,
    IDeviceProfile,
    IDeviceSignInRequest,
    IDeviceStatus,
    IPairDeviceRequest,
    IPairDeviceResponse,
    IPairingCode,
    IRevokeDeviceRequest,
} from 'src/lib/api/devices/devicesTypes';

/**
 * Endpoints of DevicesController. The device itself is identified by the HttpOnly cookie mw_device;
 * pairingCode, list and revoke are for adults (LP-106).
 */
export const devicesApi = {
    status: statelessApi.create<IDeviceStatus>({ serviceUrl: '/devices/status' }),
    pair: statelessApi.create<IPairDeviceResponse, IPairDeviceRequest>({
        serviceUrl: '/devices/pair',
    }),
    profiles: statelessApi.create<IDeviceProfile[]>({ serviceUrl: '/devices/profiles' }),
    signIn: statelessApi.create<ISession, IDeviceSignInRequest>({
        serviceUrl: '/devices/sign-in',
    }),
    pairingCode: statelessApi.create<IPairingCode>({ serviceUrl: '/devices/pairing-code' }),
    list: statelessApi.create<IDevice[]>({ serviceUrl: '/devices/list' }),
    revoke: statelessApi.create<void, IRevokeDeviceRequest>({ serviceUrl: '/devices/revoke' }),
};
