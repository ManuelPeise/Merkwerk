import { statelessApi } from 'src/lib/api/StatelessApi';
import type {
    IConfirmEmailRequest,
    ICurrentUser,
    IForgotPasswordRequest,
    ILoginRequest,
    IResetPasswordRequest,
    ISession,
} from 'src/lib/api/authentication/authenticationTypes';

/** Endpoints of AuthenticationController. Paths are lowercase on purpose (refresh cookie path). */
export const authenticationApi = {
    login: statelessApi.create<ISession, ILoginRequest>({ serviceUrl: '/authentication/login' }),
    logout: statelessApi.create<void>({ serviceUrl: '/authentication/logout' }),
    me: statelessApi.create<ICurrentUser>({ serviceUrl: '/authentication/me' }),
    forgotPassword: statelessApi.create<void, IForgotPasswordRequest>({
        serviceUrl: '/authentication/forgot-password',
    }),
    resetPassword: statelessApi.create<void, IResetPasswordRequest>({
        serviceUrl: '/authentication/reset-password',
    }),
    confirmEmail: statelessApi.create<void, IConfirmEmailRequest>({
        serviceUrl: '/authentication/confirm-email',
    }),
};
