import { statelessApi } from 'src/lib/api/StatelessApi';
import type {
    ICurrentUser,
    ILoginRequest,
    ISession,
} from 'src/lib/api/authentication/authenticationTypes';

/** Endpoints of AuthenticationController. Paths are lowercase on purpose (refresh cookie path). */
export const authenticationApi = {
    login: statelessApi.create<ISession, ILoginRequest>({ serviceUrl: '/authentication/login' }),
    logout: statelessApi.create<void>({ serviceUrl: '/authentication/logout' }),
    me: statelessApi.create<ICurrentUser>({ serviceUrl: '/authentication/me' }),
};
