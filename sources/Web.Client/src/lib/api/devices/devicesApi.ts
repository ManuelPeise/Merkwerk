import { statelessApi } from 'src/lib/api/StatelessApi';
import type { ISession } from 'src/lib/api/authentication/authenticationTypes';
import type {
    IDeviceProfile,
    IDeviceSignInRequest,
    IDeviceStatus,
    IPairDeviceRequest,
    IPairDeviceResponse,
} from 'src/lib/api/devices/devicesTypes';

/** Endpoints of DevicesController. The device itself is identified by the HttpOnly cookie mw_device. */
export const devicesApi = {
    status: statelessApi.create<IDeviceStatus>({ serviceUrl: '/devices/status' }),
    pair: statelessApi.create<IPairDeviceResponse, IPairDeviceRequest>({
        serviceUrl: '/devices/pair',
    }),
    profiles: statelessApi.create<IDeviceProfile[]>({ serviceUrl: '/devices/profiles' }),
    signIn: statelessApi.create<ISession, IDeviceSignInRequest>({
        serviceUrl: '/devices/sign-in',
    }),
};
